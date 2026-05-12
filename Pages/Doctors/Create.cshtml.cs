using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
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

    public SelectList Specialties { get; set; } = default!;

    public IActionResult OnGet()
    {
        Doctor = new Doctor { User = new User() };
        LoadSpecialties();
        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        ModelState.Remove("Doctor.User.PasswordHash");
        ModelState.Remove("Doctor.User.Role");

        if (!ModelState.IsValid)
        {
            LoadSpecialties();
            return Page();
        }

        try
        {
            // Create user first
            var user = Doctor.User;
            user.PasswordHash = "temp-hash";
            user.Role = "Doctor";
            user.CreatedAt = DateTime.UtcNow;

            await _service.AddDoctorAsync(Doctor);

            return RedirectToPage("Index");
        }
        catch (Exception ex)
        {
            ModelState.AddModelError(string.Empty, $"Error: {ex.Message}");
            LoadSpecialties();
            return Page();
        }
    }

    private void LoadSpecialties()
    {
        Specialties = new SelectList(new List<string>
        {
            "General Practice",
            "Cardiology",
            "Neurology",
            "Pediatrics",
            "Dermatology",
            "Orthopedics",
            "Gastroenterology",
            "ENT",
            "Ophthalmology",
            "Psychiatry",
            "Endocrinology",
            "Urology"
        });
    }
}
