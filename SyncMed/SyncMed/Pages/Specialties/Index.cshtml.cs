using Microsoft.AspNetCore.Mvc.RazorPages;
using SyncMed.Services;

namespace SyncMed.Pages.Specialties;

public class IndexModel : PageModel
{
    private readonly ISpecialtyService _service;

    public IndexModel(ISpecialtyService service)
    {
        _service = service;
    }

    public IList<string> Specialties { get; set; } = default!;

    public async Task OnGetAsync()
    {
        Specialties = await _service.GetAllSpecialtiesAsync();
    }
}

