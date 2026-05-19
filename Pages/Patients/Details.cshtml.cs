using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SyncMed.Authorization;
using SyncMed.Extensions;
using SyncMed.Models;
using SyncMed.Services;

namespace SyncMed.Pages.Patients;

[Authorize(Roles = AppRoles.AnyAuthenticatedRole)]
public class DetailsModel : PageModel
{
    private readonly IPatientService _patientService;

    public DetailsModel(IPatientService patientService)
    {
        _patientService = patientService;
    }

    public Patient Patient { get; set; } = default!;

    public async Task<IActionResult> OnGetAsync(int id)
    {
        var patient = await _patientService.GetPatientDetailsAsync(id);
        if (patient == null)
            return NotFound();

        if (!CanViewPatient(patient))
            return Forbid();

        Patient = patient;
        return Page();
    }

    private bool CanViewPatient(Patient patient)
    {
        var userId = User.GetUserId();

        if (User.IsInRole(AppRoles.Admin) || User.IsInRole(AppRoles.Nurse))
            return true;

        if (User.IsInRole(AppRoles.Patient))
            return patient.PatientId == userId;

        if (User.IsInRole(AppRoles.Doctor))
            return patient.Appointments.Any(a => a.DoctorId == userId);

        return false;
    }
}
