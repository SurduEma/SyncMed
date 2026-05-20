using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SyncMed.Authorization;
using SyncMed.Models;
using SyncMed.Services;

namespace SyncMed.Pages.Doctors;

[Authorize(Roles = AppRoles.AdminOnly)]
public class DeleteModel : PageModel
{
    private readonly IDoctorService _service;

    public DeleteModel(IDoctorService service)
    {
        _service = service;
    }

    [BindProperty]
    public Doctor Doctor { get; set; } = default!;

    public async Task<IActionResult> OnGetAsync(int id)
    {
        Doctor = await _service.GetDoctorByIdAsync(id);
        if (Doctor == null)
            return NotFound();

        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        try
        {
            await _service.DeleteDoctorAsync(Doctor.DoctorId);
            return RedirectToPage("Index");
        }
        catch (Exception ex)
        {
            ModelState.AddModelError(string.Empty, $"Error: {ex.Message}");
            return Page();
        }
    }
}
