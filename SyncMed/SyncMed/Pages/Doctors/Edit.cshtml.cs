using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SyncMed.Models;
using SyncMed.Services;

namespace SyncMed.Pages.Doctors;

public class EditModel : PageModel
{
    private readonly IDoctorService _service;

    public EditModel(IDoctorService service)
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
        if (!ModelState.IsValid)
            return Page();

        try
        {
            await _service.UpdateDoctorAsync(Doctor);
            return RedirectToPage("Index");
        }
        catch (Exception ex)
        {
            ModelState.AddModelError(string.Empty, $"Error: {ex.Message}");
            return Page();
        }
    }
}
