using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SyncMed.Authorization;
using SyncMed.Extensions;
using SyncMed.Models;
using SyncMed.Services;

namespace SyncMed.Pages.Prescriptions;

[Authorize(Roles = AppRoles.AnyAuthenticatedRole)]
public class IndexModel : PageModel
{
    private readonly IClinicalRecordService _clinicalRecordService;

    public IndexModel(IClinicalRecordService clinicalRecordService)
    {
        _clinicalRecordService = clinicalRecordService;
    }

    public IList<Prescription> Prescriptions { get; set; } = new List<Prescription>();

    public async Task OnGetAsync()
    {
        await LoadPrescriptionsAsync();
    }

    public async Task<IActionResult> OnPostApproveAsync(int id)
    {
        var prescription = await _clinicalRecordService.GetPrescriptionByIdAsync(id);
        if (prescription == null)
            return NotFound();

        if (!CanApprove(prescription))
            return Forbid();

        var doctorId = User.IsInRole(AppRoles.Admin) ? prescription.DoctorId : User.GetUserId() ?? 0;
        var result = await _clinicalRecordService.ApprovePrescriptionAsync(id, doctorId);
        if (!result.Success)
        {
            ModelState.AddModelError(string.Empty, result.Message);
            await LoadPrescriptionsAsync();
            return Page();
        }

        return RedirectToPage();
    }

    public bool CanApprove(Prescription prescription)
    {
        return prescription.Status == "Draft"
            && (User.IsInRole(AppRoles.Admin)
                || (User.IsInRole(AppRoles.Doctor) && prescription.DoctorId == User.GetUserId()));
    }

    private async Task LoadPrescriptionsAsync()
    {
        var userId = User.GetUserId();
        var prescriptions = await _clinicalRecordService.GetPrescriptionsAsync();

        if (User.IsInRole(AppRoles.Patient))
        {
            prescriptions = prescriptions.Where(p => p.PatientId == userId).ToList();
        }
        else if (User.IsInRole(AppRoles.Doctor))
        {
            prescriptions = prescriptions.Where(p => p.DoctorId == userId).ToList();
        }
        else if (User.IsInRole(AppRoles.Nurse))
        {
            prescriptions = prescriptions.Where(p => p.DraftedByNurseId == userId).ToList();
        }

        Prescriptions = prescriptions;
    }
}
