using Microsoft.AspNetCore.Mvc.RazorPages;
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

    public async Task OnGetAsync()
    {
        Doctors = await _service.GetAllDoctorsAsync();
    }
}
