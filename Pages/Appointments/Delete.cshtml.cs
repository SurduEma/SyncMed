using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SyncMed.Authorization;
using SyncMed.Extensions;
using SyncMed.Models;
using SyncMed.Services;

namespace SyncMed.Pages.Appointments;

[Authorize(Roles = AppRoles.PatientOrAdmin)]
public class DeleteModel : PageModel
{
    private readonly IAppointmentService _service;

    public DeleteModel(IAppointmentService service)
    {
        _service = service;
    }

    [BindProperty]
    public Appointment Appointment { get; set; } = default!;

    public async Task<IActionResult> OnGetAsync(int id)
    {
        Appointment = await _service.GetAppointmentByIdAsync(id);
        if (Appointment == null)
            return NotFound();

        if (!CanAccessAppointment(Appointment))
            return Forbid();

        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        try
        {
            var existing = await _service.GetAppointmentByIdAsync(Appointment.AppointmentId);
            if (existing == null)
                return NotFound();

            if (!CanAccessAppointment(existing))
                return Forbid();

            var result = await _service.CancelAppointmentAsync(existing.AppointmentId);
            if (!result.Success)
            {
                ModelState.AddModelError(string.Empty, result.Message);
                Appointment = existing;
                return Page();
            }
            return RedirectToPage("Index");
        }
        catch (Exception ex)
        {
            ModelState.AddModelError(string.Empty, $"Error: {ex.Message}");
            return Page();
        }
    }

    private bool CanAccessAppointment(Appointment appointment)
    {
        return User.IsInRole(AppRoles.Admin)
            || (User.IsInRole(AppRoles.Patient) && appointment.PatientId == User.GetUserId());
    }
}
