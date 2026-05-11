using Microsoft.AspNetCore.Mvc.RazorPages;
using SyncMed.Models;
using SyncMed.Services;

namespace SyncMed.Pages.Patients;

public class IndexModel : PageModel
{
    private readonly IPatientService _service;

    public IndexModel(IPatientService service)
    {
        _service = service;
    }

    public IList<Patient> Patients { get; set; } = default!;

    public async Task OnGetAsync()
    {
        Patients = await _service.GetAllPatientsAsync();
    }
}
