using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SyncMed.Models;
using SyncMed.Services;

namespace SyncMed.Pages.Doctors;

public class CreateModel : PageModel
{
    private readonly IDoctorService _service;

    public CreateModel(IDoctorService service)
    {
        _service = service;
    }

    [BindProperty]
    public Doctor Doctor { get; set; } = default!;

    public IActionResult OnGet()
    {
        Doctor = new Doctor { User = new User() };
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
            var user = Doctor.User;
            user.PasswordHash = "temp-hash";
            user.Role = "Doctor";
            user.CreatedAt = DateTime.UtcNow;

            // We need to manually handle user creation via a service or context
            // For now, we'll use the doctor service which should handle this
            await _service.AddDoctorAsync(Doctor);

            return RedirectToPage("Index");
        }
        catch (Exception ex)
        {
            ModelState.AddModelError(string.Empty, $"Error: {ex.Message}");
            return Page();
        }
    }
}
