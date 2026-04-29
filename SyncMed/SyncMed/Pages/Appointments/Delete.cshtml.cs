using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SyncMed.Models;
using SyncMed.Services;

namespace SyncMed.Pages.Appointments;

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

        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        try
        {
            var result = await _service.CancelAppointmentAsync(Appointment.AppointmentId);
            if (!result.Success)
            {
                ModelState.AddModelError(string.Empty, result.Message);
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
}
