using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using SyncMed.Models;
using SyncMed.Services;

namespace SyncMed.Pages.Doctors;

public class IndexModel : PageModel
{
    private readonly IDoctorService _service;

    public IndexModel(IDoctorService service)
    {
        _service = service;
    }

    public IList<Doctor> Doctors { get; set; } = default!;

    [BindProperty(SupportsGet = true)]
    public string? Specialty { get; set; }

    public SelectList Specialties { get; set; } = default!;

    private static readonly List<string> AllSpecialties = new()
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
    };

    public async Task OnGetAsync()
    {
        if (string.IsNullOrEmpty(Specialty))
        {
            Specialty = AllSpecialties.First();
        }

        Specialties = new SelectList(AllSpecialties, Specialty);

        var allDoctors = await _service.GetAllDoctorsAsync();

        Doctors = allDoctors.Where(d => string.Equals(d.Specialty, Specialty, StringComparison.OrdinalIgnoreCase)).ToList();
    }
}
