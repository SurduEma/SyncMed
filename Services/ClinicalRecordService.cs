using Microsoft.EntityFrameworkCore;
using SyncMed.Data;
using SyncMed.Models;

namespace SyncMed.Services;

public interface IClinicalRecordService
{
    Task<Consultation?> GetConsultationByAppointmentIdAsync(int appointmentId);
    Task<(bool Success, string Message)> RecordVitalsAsync(int appointmentId, int nurseId, string? vitals, string? symptoms);
    Task<(bool Success, string Message)> SaveDoctorNotesAsync(int appointmentId, string? symptoms, string? diagnosis);
    Task<IList<Prescription>> GetPrescriptionsAsync();
    Task<Prescription?> GetPrescriptionByIdAsync(int id);
    Task<(bool Success, string Message)> CreatePrescriptionAsync(int patientId, int doctorId, int? draftedByNurseId, string medicationDetails, string status);
    Task<(bool Success, string Message)> ApprovePrescriptionAsync(int prescriptionId, int doctorId);
    Task<(bool Success, string Message)> AddTestResultAsync(int patientId, int uploadedByDoctorId, string testName, string documentUrl);
}

public class ClinicalRecordService : IClinicalRecordService
{
    private readonly SyncMedDbContext _context;

    public ClinicalRecordService(SyncMedDbContext context)
    {
        _context = context;
    }

    public async Task<Consultation?> GetConsultationByAppointmentIdAsync(int appointmentId)
    {
        return await _context.Consultations
            .Include(c => c.Appointment)
                .ThenInclude(a => a.Patient)
                    .ThenInclude(p => p.User)
            .Include(c => c.Nurse)
                .ThenInclude(n => n.User)
            .FirstOrDefaultAsync(c => c.AppointmentId == appointmentId);
    }

    public async Task<(bool Success, string Message)> RecordVitalsAsync(int appointmentId, int nurseId, string? vitals, string? symptoms)
    {
        var appointmentExists = await _context.Appointments.AnyAsync(a => a.AppointmentId == appointmentId);
        if (!appointmentExists)
            return (false, "Appointment not found.");

        if (nurseId == 0)
            nurseId = await _context.Nurses.Select(n => n.NurseId).FirstOrDefaultAsync();

        var nurseExists = nurseId != 0 && await _context.Nurses.AnyAsync(n => n.NurseId == nurseId);
        if (!nurseExists)
            return (false, "Nurse profile not found.");

        var consultation = await _context.Consultations.FirstOrDefaultAsync(c => c.AppointmentId == appointmentId);
        if (consultation == null)
        {
            consultation = new Consultation
            {
                AppointmentId = appointmentId,
                NurseId = nurseId,
                CreatedAt = DateTime.UtcNow
            };
            _context.Consultations.Add(consultation);
        }

        consultation.NurseId = nurseId;
        consultation.Vitals = vitals;
        consultation.Symptoms = symptoms;

        await _context.SaveChangesAsync();
        return (true, "Vitals recorded.");
    }

    public async Task<(bool Success, string Message)> SaveDoctorNotesAsync(int appointmentId, string? symptoms, string? diagnosis)
    {
        var appointmentExists = await _context.Appointments.AnyAsync(a => a.AppointmentId == appointmentId);
        if (!appointmentExists)
            return (false, "Appointment not found.");

        var consultation = await _context.Consultations.FirstOrDefaultAsync(c => c.AppointmentId == appointmentId);
        if (consultation == null)
        {
            var nurseId = await _context.Nurses.Select(n => n.NurseId).FirstOrDefaultAsync();
            if (nurseId == 0)
                return (false, "At least one nurse account is required before a consultation can be opened.");

            consultation = new Consultation
            {
                AppointmentId = appointmentId,
                NurseId = nurseId,
                CreatedAt = DateTime.UtcNow
            };
            _context.Consultations.Add(consultation);
        }

        consultation.Symptoms = symptoms;
        consultation.Diagnosis = diagnosis;

        await _context.SaveChangesAsync();
        return (true, "Consultation notes saved.");
    }

    public async Task<IList<Prescription>> GetPrescriptionsAsync()
    {
        return await _context.Prescriptions
            .Include(p => p.Patient)
                .ThenInclude(p => p.User)
            .Include(p => p.Doctor)
                .ThenInclude(d => d.User)
            .Include(p => p.DraftedByNurse)
                .ThenInclude(n => n!.User)
            .OrderByDescending(p => p.DateIssued)
            .ToListAsync();
    }

    public async Task<Prescription?> GetPrescriptionByIdAsync(int id)
    {
        return await _context.Prescriptions
            .Include(p => p.Patient)
                .ThenInclude(p => p.User)
            .Include(p => p.Doctor)
                .ThenInclude(d => d.User)
            .Include(p => p.DraftedByNurse)
                .ThenInclude(n => n!.User)
            .FirstOrDefaultAsync(p => p.PrescriptionId == id);
    }

    public async Task<(bool Success, string Message)> CreatePrescriptionAsync(
        int patientId,
        int doctorId,
        int? draftedByNurseId,
        string medicationDetails,
        string status)
    {
        if (!await _context.Patients.AnyAsync(p => p.PatientId == patientId))
            return (false, "Patient not found.");

        if (!await _context.Doctors.AnyAsync(d => d.DoctorId == doctorId))
            return (false, "Doctor not found.");

        if (draftedByNurseId.HasValue && !await _context.Nurses.AnyAsync(n => n.NurseId == draftedByNurseId))
            return (false, "Nurse not found.");

        _context.Prescriptions.Add(new Prescription
        {
            PatientId = patientId,
            DoctorId = doctorId,
            DraftedByNurseId = draftedByNurseId,
            MedicationDetails = medicationDetails.Trim(),
            Status = status,
            DateIssued = DateTime.UtcNow
        });

        await _context.SaveChangesAsync();
        return (true, status == "Draft" ? "Prescription draft saved." : "Prescription issued.");
    }

    public async Task<(bool Success, string Message)> ApprovePrescriptionAsync(int prescriptionId, int doctorId)
    {
        var prescription = await _context.Prescriptions.FindAsync(prescriptionId);
        if (prescription == null)
            return (false, "Prescription not found.");

        if (prescription.DoctorId != doctorId)
            return (false, "Only the assigned doctor can approve this prescription.");

        prescription.Status = "Approved";
        prescription.DateIssued = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return (true, "Prescription approved.");
    }

    public async Task<(bool Success, string Message)> AddTestResultAsync(int patientId, int uploadedByDoctorId, string testName, string documentUrl)
    {
        if (!await _context.Patients.AnyAsync(p => p.PatientId == patientId))
            return (false, "Patient not found.");

        if (!await _context.Doctors.AnyAsync(d => d.DoctorId == uploadedByDoctorId))
            return (false, "Doctor not found.");

        _context.TestResults.Add(new TestResult
        {
            PatientId = patientId,
            UploadedByDoctorId = uploadedByDoctorId,
            TestName = testName.Trim(),
            DocumentUrl = documentUrl.Trim(),
            DateUploaded = DateTime.UtcNow
        });

        await _context.SaveChangesAsync();
        return (true, "Test result uploaded.");
    }
}
