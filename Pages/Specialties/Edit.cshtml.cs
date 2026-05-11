using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SyncMed.Models;
using SyncMed.Services;

namespace SyncMed.Pages.Specialties;

public class EditModel : PageModel
{
    private readonly IDoctorService _doctorService;

    public EditModel(IDoctorService doctorService)
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
        if (!ModelState.IsValid)
            return Page();

        try
        {
            await _doctorService.UpdateDoctorAsync(Doctor);
            return RedirectToPage("Index");
        }
        catch (Exception ex)
        {
            ModelState.AddModelError(string.Empty, $"Error: {ex.Message}");
            return Page();
        }
    }
}
