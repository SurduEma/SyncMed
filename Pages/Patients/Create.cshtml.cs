using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SyncMed.Models;
using SyncMed.Services;

namespace SyncMed.Pages.Patients;

public class CreateModel : PageModel
{
    private readonly IPatientService _service;

    public CreateModel(IPatientService service)
    {
        _service = service;
    }

    [BindProperty]
    public Patient Patient { get; set; } = default!;

    public IActionResult OnGet()
    {
        Patient = new Patient { User = new User() };
        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        try
        {
            // Create user first
            var user = Patient.User;
            user.PasswordHash = "temp-hash";
            user.Role = "Patient";
            user.CreatedAt = DateTime.UtcNow;

            await _service.AddPatientAsync(Patient);
            return RedirectToPage("Index");
        }
        catch (Exception ex)
        {
            ModelState.AddModelError(string.Empty, $"Error: {ex.Message}");
            return Page();
        }
    }
}
