using Microsoft.AspNetCore.Mvc.RazorPages;
using SyncMed.Models;
using SyncMed.Services;

namespace SyncMed.Pages.Specialties;

public class IndexModel : PageModel
{
    private readonly ISpecialtyService _specialtyService;
    private readonly IDoctorService _doctorService;

    public IndexModel(ISpecialtyService specialtyService, IDoctorService doctorService)
    {
        _specialtyService = specialtyService;
        _doctorService = doctorService;
    }

    public IList<string> Specialties { get; set; } = default!;
    public Dictionary<string, IList<Doctor>> DoctorsBySpecialty { get; set; } = new();

    public async Task OnGetAsync()
    {
        Specialties = await _specialtyService.GetAllSpecialtiesAsync();

        // Load all doctors
        var allDoctors = await _doctorService.GetAllDoctorsAsync();

        // Group doctors by specialty
        foreach (var specialty in Specialties)
        {
            var doctorsInSpecialty = allDoctors
                .Where(d => d.Specialty == specialty)
                .ToList();
            DoctorsBySpecialty[specialty] = doctorsInSpecialty;
        }
    }
}
