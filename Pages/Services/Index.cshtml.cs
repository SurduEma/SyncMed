using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using SyncMed.Models;
using SyncMed.Services;

namespace SyncMed.Pages.Services;

public class IndexModel : PageModel
{
    private readonly IMedicalServiceModelService _service;

    public IndexModel(IMedicalServiceModelService service)
    {
        _service = service;
    }

    public IList<MedicalService> Services { get; set; } = default!;

    [BindProperty(SupportsGet = true)]
    public string? Specialty { get; set; }

    public SelectList Specialties { get; set; } = default!;

    public static readonly List<string> AllSpecialties = new()
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

        var allServices = await _service.GetAllServicesAsync();

        Services = allServices.Where(s => string.Equals(s.Specialty, Specialty, StringComparison.OrdinalIgnoreCase)).ToList();
    }
}
