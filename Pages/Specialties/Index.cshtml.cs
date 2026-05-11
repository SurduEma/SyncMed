using Microsoft.AspNetCore.Mvc.RazorPages;
using SyncMed.Models;
using SyncMed.Services;

namespace SyncMed.Pages.Specialties;

public class SpecialtyInfo
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string ImagePath { get; set; } = string.Empty;
}

public class IndexModel : PageModel
{
    private readonly ISpecialtyService _specialtyService;

    public IndexModel(ISpecialtyService specialtyService)
    {
        _specialtyService = specialtyService;
    }

    public IList<SpecialtyInfo> SpecialtiesInfo { get; set; } = new List<SpecialtyInfo>();

    private static readonly Dictionary<string, (string description, string image)> SpecialtyMetadata = new()
    {
        { "General Practice", ("Comprehensive primary healthcare and general medical services for patients of all ages.", "general.jpg") },
        { "Cardiology", ("Specializing in heart and cardiovascular system diseases, treatments, and preventive care.", "cardiology.jpg") },
        { "Neurology", ("Expert care for nervous system disorders including brain, spinal cord, and nerve conditions.", "neurology.jpg") },
        { "Pediatrics", ("Specialized medical care and treatment for infants, children, and adolescents.", "pediatrics.jpg") },
        { "Dermatology", ("Comprehensive skin care and treatment of dermatological conditions and diseases.", "dermatology.jpg") },
        { "Orthopedics", ("Specialized care for bones, joints, ligaments, and musculoskeletal system disorders.", "orthopedics.jpg") },
        { "Gastroenterology", ("Specialist care for digestive system and gastrointestinal disorders and diseases.", "gastroenterology.jpg") },
        { "ENT", ("Otolaryngology specializing in ear, nose, and throat conditions and treatments.", "ent.jpg") },
        { "Ophthalmology", ("Specialized eye care and treatment of vision and ocular disorders.", "ophtalmology.jpg") },
        { "Psychiatry", ("Mental health and psychological disorder diagnosis and treatment services.", "psychiatry.jpg") },
        { "Endocrinology", ("Specialist care for hormonal and endocrine system disorders.", "endocrinology.jpg") },
        { "Urology", ("Specialized care for urinary system and male reproductive health disorders.", "urology.jpg") }
    };

    public async Task OnGetAsync()
    {
        var specialties = SpecialtyMetadata.Keys.ToList();

        foreach (var specialty in specialties)
        {
            var metadata = SpecialtyMetadata[specialty];

            SpecialtiesInfo.Add(new SpecialtyInfo
            {
                Name = specialty,
                Description = metadata.description,
                ImagePath = $"/assets/specialties-img/{metadata.image}"
            });
        }

        await Task.CompletedTask;
    }
}
