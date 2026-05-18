using SyncMed.Data.Repositories;
using SyncMed.Models;

namespace SyncMed.Services;

public interface IAppointmentService
{
    Task<IList<Appointment>> GetAllAppointmentsAsync();
    Task<Appointment?> GetAppointmentByIdAsync(int id);
    Task<(bool Success, string Message)> BookAppointmentAsync(int patientId, int doctorId, DateOnly date, TimeOnly time);
    Task UpdateAppointmentAsync(Appointment appointment);
    Task<(bool Success, string Message)> CancelAppointmentAsync(int appointmentId);
    Task<(bool Success, string Message)> ConfirmAppointmentAsync(int appointmentId);
    Task<List<TimeOnly>> GetAvailableTimeSlotsAsync(int doctorId, DateOnly date);
}

public class AppointmentService : IAppointmentService
{
    private readonly IAppointmentRepository _appointmentRepository;
    private readonly IDoctorRepository _doctorRepository;
    private readonly IPatientRepository _patientRepository;
    private readonly IAppointmentNotificationService _notificationService;
    private const int AppointmentDurationMinutes = 30;
    private static readonly TimeOnly DefaultWorkingHoursStart = new(9, 0);
    private static readonly TimeOnly DefaultWorkingHoursEnd = new(20, 0);

    public AppointmentService(
        IAppointmentRepository appointmentRepository,
        IDoctorRepository doctorRepository,
        IPatientRepository patientRepository,
        IAppointmentNotificationService notificationService)
    {
        _appointmentRepository = appointmentRepository;
        _doctorRepository = doctorRepository;
        _patientRepository = patientRepository;
        _notificationService = notificationService;
    }

    public async Task<IList<Appointment>> GetAllAppointmentsAsync()
    {
        return await _appointmentRepository.GetAllWithIncludesAsync();
    }

    public async Task<Appointment?> GetAppointmentByIdAsync(int id)
    {
        return await _appointmentRepository.GetByIdWithIncludesAsync(id);
    }

    public async Task<(bool Success, string Message)> BookAppointmentAsync(
        int patientId, int doctorId, DateOnly date, TimeOnly time)
    {
        // Validate date is not in the past
        if (date < DateOnly.FromDateTime(DateTime.Today))
            return (false, "Cannot book appointment in the past.");

        if (date.DayOfWeek is DayOfWeek.Saturday or DayOfWeek.Sunday)
            return (false, "Appointments cannot be booked on weekends.");

        // Validate time slot is on 30-minute intervals (00 or 30 minutes)
        if (time.Minute != 0 && time.Minute != 30)
            return (false, "Appointments must start at :00 or :30 minutes.");

        // Validate time is not in the past (if today)
        if (date == DateOnly.FromDateTime(DateTime.Today) && time <= TimeOnly.FromDateTime(DateTime.Now))
            return (false, "Cannot book appointment at a time that has already passed.");

        // Validate patient exists
        var patient = await _patientRepository.GetByIdAsync(patientId);
        if (patient == null)
            return (false, "Patient not found.");

        // Validate doctor exists
        var doctor = await _doctorRepository.GetByIdAsync(doctorId);
        if (doctor == null)
            return (false, "Doctor not found.");

        var (workingStart, workingEnd) = GetWorkingHours(doctor);
        if (time < workingStart || time >= workingEnd)
            return (false, $"Appointments with this doctor must be booked between {workingStart:HH\\:mm} and {workingEnd:HH\\:mm}.");

        // Check for time-slot overlap
        var sameDayAppointments = await _appointmentRepository.GetByDoctorAndDateAsync(doctorId, date);
        var slotDuration = TimeSpan.FromMinutes(AppointmentDurationMinutes);
        var requestedStart = time;
        var requestedEnd = time.Add(slotDuration);

        bool hasConflict = sameDayAppointments.Any(existing =>
            requestedStart < existing.AppointmentTime.Add(slotDuration)
            && requestedEnd > existing.AppointmentTime);

        if (hasConflict)
            return (false, $"This time slot overlaps with an existing appointment. Each appointment lasts {AppointmentDurationMinutes} minutes - please choose a different time.");

        // Create and save appointment
        var appointment = new Appointment
        {
            PatientId = patientId,
            DoctorId = doctorId,
            AppointmentDate = date,
            AppointmentTime = time,
            Status = "Pending"
        };

        await _appointmentRepository.AddAsync(appointment);
        appointment.Patient = patient;
        appointment.Doctor = doctor;
        await _notificationService.SendAppointmentConfirmationAsync(appointment);

        return (true, "Appointment booked successfully!");
    }

    public async Task UpdateAppointmentAsync(Appointment appointment)
    {
        var existingAppointment = await _appointmentRepository.GetByIdAsync(appointment.AppointmentId);
        if (existingAppointment == null)
            throw new InvalidOperationException("Appointment not found.");

        existingAppointment.PatientId = appointment.PatientId;
        existingAppointment.DoctorId = appointment.DoctorId;
        existingAppointment.AppointmentDate = appointment.AppointmentDate;
        existingAppointment.AppointmentTime = appointment.AppointmentTime;
        existingAppointment.Status = appointment.Status;

        await _appointmentRepository.UpdateAsync(existingAppointment);
    }

    public async Task<(bool Success, string Message)> CancelAppointmentAsync(int appointmentId)
    {
        var appointment = await _appointmentRepository.GetByIdAsync(appointmentId);
        if (appointment == null)
            return (false, "Appointment not found.");

        if (appointment.Status == "Cancelled")
            return (false, "Appointment is already cancelled.");

        appointment.Status = "Cancelled";
        await _appointmentRepository.UpdateAsync(appointment);
        return (true, "Appointment cancelled successfully.");
    }

    public async Task<(bool Success, string Message)> ConfirmAppointmentAsync(int appointmentId)
    {
        var appointment = await _appointmentRepository.GetByIdAsync(appointmentId);
        if (appointment == null)
            return (false, "Appointment not found.");

        if (appointment.Status == "Confirmed")
            return (false, "Appointment is already confirmed.");

        appointment.Status = "Confirmed";
        await _appointmentRepository.UpdateAsync(appointment);
        return (true, "Appointment confirmed successfully.");
    }

    public async Task<List<TimeOnly>> GetAvailableTimeSlotsAsync(int doctorId, DateOnly date)
    {
        var availableSlots = new List<TimeOnly>();
        if (date.DayOfWeek is DayOfWeek.Saturday or DayOfWeek.Sunday)
            return availableSlots;

        var doctor = await _doctorRepository.GetByIdAsync(doctorId);
        if (doctor == null)
            return availableSlots;

        var (workingStart, workingEnd) = GetWorkingHours(doctor);
        for (var slot = workingStart; slot < workingEnd; slot = slot.Add(TimeSpan.FromMinutes(AppointmentDurationMinutes)))
        {
            availableSlots.Add(slot);
        }

        // Get all appointments for this doctor on this date
        var bookedAppointments = await _appointmentRepository.GetByDoctorAndDateAsync(doctorId, date);

        // Remove booked slots and overlapping slots
        var slotDuration = TimeSpan.FromMinutes(AppointmentDurationMinutes);
        availableSlots = availableSlots
            .Where(slot =>
            {
                var slotEnd = slot.Add(slotDuration);
                // Check if this slot conflicts with any existing appointment
                return !bookedAppointments.Any(apt =>
                    slot < apt.AppointmentTime.Add(slotDuration) && slotEnd > apt.AppointmentTime);
            })
            .ToList();

        // Filter out past times if booking for today
        if (date == DateOnly.FromDateTime(DateTime.Today))
        {
            var now = TimeOnly.FromDateTime(DateTime.Now);
            availableSlots = availableSlots.Where(slot => slot > now).ToList();
        }

        return availableSlots;
    }

    private static (TimeOnly Start, TimeOnly End) GetWorkingHours(Doctor doctor)
    {
        if (!string.IsNullOrWhiteSpace(doctor.WorkingHours))
        {
            var match = System.Text.RegularExpressions.Regex.Match(
                doctor.WorkingHours,
                @"(?<start>\d{1,2}:\d{2})\s*-\s*(?<end>\d{1,2}:\d{2})");

            if (match.Success
                && TimeOnly.TryParse(match.Groups["start"].Value, out var start)
                && TimeOnly.TryParse(match.Groups["end"].Value, out var end)
                && start < end)
            {
                return (start, end);
            }
        }

        return (DefaultWorkingHoursStart, DefaultWorkingHoursEnd);
    }
}
