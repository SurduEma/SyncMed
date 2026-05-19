using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using SyncMed.Authorization;
using SyncMed.Extensions;
using SyncMed.Services;

namespace SyncMed.Pages.Prescriptions;

[Authorize(Roles = AppRoles.StaffOrAdmin)]
public class CreateModel : PageModel
{
    private readonly IClinicalRecordService _clinicalRecordService;
    private readonly IPatientService _patientService;
    private readonly IDoctorService _doctorService;

    public CreateModel(
        IClinicalRecordService clinicalRecordService,
        IPatientService patientService,
        IDoctorService doctorService)
    {
        _clinicalRecordService = clinicalRecordService;
        _patientService = patientService;
        _doctorService = doctorService;
    }

    [BindProperty(SupportsGet = true)]
    public int PatientId { get; set; }

    [BindProperty]
    public int DoctorId { get; set; }

    [BindProperty, Required, MaxLength(4000)]
    public string MedicationDetails { get; set; } = string.Empty;

    [BindProperty]
    public string Status { get; set; } = "Approved";

    public SelectList PatientList { get; set; } = default!;
    public SelectList DoctorList { get; set; } = default!;

    public async Task OnGetAsync()
    {
        await LoadDropdownsAsync();
        if (User.IsInRole(AppRoles.Doctor))
            DoctorId = User.GetUserId() ?? 0;
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (User.IsInRole(AppRoles.Doctor))
        {
            DoctorId = User.GetUserId() ?? 0;
            Status = "Approved";
        }
        else if (User.IsInRole(AppRoles.Nurse))
        {
            Status = "Draft";
        }
        else if (Status is not ("Draft" or "Approved"))
        {
            ModelState.AddModelError(nameof(Status), "Please select a valid status.");
        }

        if (PatientId == 0)
            ModelState.AddModelError(nameof(PatientId), "Please select a patient.");

        if (DoctorId == 0)
            ModelState.AddModelError(nameof(DoctorId), "Please select a doctor.");

        if (User.IsInRole(AppRoles.Doctor))
        {
            var patient = await _patientService.GetPatientDetailsAsync(PatientId);
            if (patient == null || !patient.Appointments.Any(a => a.DoctorId == DoctorId))
                ModelState.AddModelError(nameof(PatientId), "Doctors can prescribe only for their own patients.");
        }

        if (!ModelState.IsValid)
        {
            await LoadDropdownsAsync();
            return Page();
        }

        var nurseId = User.IsInRole(AppRoles.Nurse) ? User.GetUserId() : null;
        var result = await _clinicalRecordService.CreatePrescriptionAsync(
            PatientId,
            DoctorId,
            nurseId,
            MedicationDetails,
            Status);

        if (!result.Success)
        {
            ModelState.AddModelError(string.Empty, result.Message);
            await LoadDropdownsAsync();
            return Page();
        }

        return RedirectToPage("/Patients/Details", new { id = PatientId });
    }

    private async Task LoadDropdownsAsync()
    {
        var patients = await _patientService.GetAllPatientsAsync();
        PatientList = new SelectList(
            patients.Select(p => new { p.PatientId, Display = $"{p.User.FirstName} {p.User.LastName}" }),
            "PatientId",
            "Display",
            PatientId);

        var doctors = await _doctorService.GetAllDoctorsAsync();
        DoctorList = new SelectList(
            doctors.Select(d => new { d.DoctorId, Display = $"Dr. {d.User.FirstName} {d.User.LastName} - {d.Specialty}" }),
            "DoctorId",
            "Display",
            DoctorId);
    }
}
