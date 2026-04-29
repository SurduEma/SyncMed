using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SyncMed.Models;
using SyncMed.Services;

namespace SyncMed.Pages.Specialties;

public class DeleteModel : PageModel
{
    private readonly IDoctorService _doctorService;

    public DeleteModel(IDoctorService doctorService)
    {
        _doctorService = doctorService;
    }

    [BindProperty]
    public Doctor Doctor { get; set; } = default!;

    public async Task<IActionResult> OnGetAsync(int id)
    {
        Doctor = await _doctorService.GetDoctorByIdAsync(id);
        if (Doctor == null)
            return NotFound();

        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        try
        {
            await _doctorService.DeleteDoctorAsync(Doctor.DoctorId);
            return RedirectToPage("Index");
        }
        catch (Exception ex)
        {
            ModelState.AddModelError(string.Empty, $"Error: {ex.Message}");
            return Page();
        }
    }
}
