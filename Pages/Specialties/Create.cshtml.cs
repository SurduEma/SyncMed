using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SyncMed.Models;
using SyncMed.Services;

namespace SyncMed.Pages.Specialties;

public class CreateModel : PageModel
{
    private readonly IDoctorService _doctorService;

    public CreateModel(IDoctorService doctorService)
    {
        _doctorService = doctorService;
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
            var user = Doctor.User;
            user.PasswordHash = "temp-hash";
            user.Role = "Doctor";
            user.CreatedAt = DateTime.UtcNow;

            await _doctorService.AddDoctorAsync(Doctor);
            return RedirectToPage("Index");
        }
        catch (Exception ex)
        {
            ModelState.AddModelError(string.Empty, $"Error: {ex.Message}");
            return Page();
        }
    }
}
