using Microsoft.AspNetCore.Mvc;
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

    [BindProperty(SupportsGet = true)]
    public string? SearchString { get; set; }

    public async Task OnGetAsync()
    {
        var patients = await _service.GetAllPatientsAsync();

        if (!string.IsNullOrEmpty(SearchString))
        {
            patients = patients.Where(p => 
                p.User.FirstName.Contains(SearchString, StringComparison.OrdinalIgnoreCase) ||
                p.User.LastName.Contains(SearchString, StringComparison.OrdinalIgnoreCase) ||
                p.User.Email.Contains(SearchString, StringComparison.OrdinalIgnoreCase) ||
                (p.PhoneNumber != null && p.PhoneNumber.Contains(SearchString, StringComparison.OrdinalIgnoreCase))
            ).ToList();
        }

        Patients = patients;
    }
}
