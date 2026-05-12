using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
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

    public SelectList Specialties { get; set; } = default!;

    public async Task<IActionResult> OnGetAsync(int id)
    {
        Doctor = await _service.GetDoctorByIdAsync(id);
        if (Doctor == null)
            return NotFound();

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
            await _service.UpdateDoctorAsync(Doctor);
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
