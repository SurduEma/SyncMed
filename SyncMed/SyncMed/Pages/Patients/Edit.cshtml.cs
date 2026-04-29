using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SyncMed.Models;
using SyncMed.Services;

namespace SyncMed.Pages.Patients;

public class EditModel : PageModel
{
    private readonly IPatientService _service;

    public EditModel(IPatientService service)
    {
        _service = service;
    }

    [BindProperty]
    public Patient Patient { get; set; } = default!;

    public async Task<IActionResult> OnGetAsync(int id)
    {
        Patient = await _service.GetPatientByIdAsync(id);
        if (Patient == null)
            return NotFound();

        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
            return Page();

        try
        {
            await _service.UpdatePatientAsync(Patient);
            return RedirectToPage("Index");
        }
        catch (Exception ex)
        {
            ModelState.AddModelError(string.Empty, $"Error: {ex.Message}");
            return Page();
        }
    }
}
