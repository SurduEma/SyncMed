using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SyncMed.Authorization;
using SyncMed.Extensions;
using SyncMed.Models;
using SyncMed.Services;

namespace SyncMed.Pages.Patients;

[Authorize(Roles = AppRoles.StaffOrAdmin)]
public class IndexModel : PageModel
{
    private readonly IPatientService _service;
    private readonly IAppointmentService _appointmentService;

    public IndexModel(IPatientService service, IAppointmentService appointmentService)
    {
        _service = service;
        _appointmentService = appointmentService;
    }

    public IList<Patient> Patients { get; set; } = default!;

    [BindProperty(SupportsGet = true)]
    public string? SearchString { get; set; }

    public async Task OnGetAsync()
    {
        var patients = await _service.GetAllPatientsAsync();

        if (User.IsInRole(AppRoles.Doctor))
        {
            var doctorId = User.GetUserId();
            var appointments = await _appointmentService.GetAllAppointmentsAsync();
            var patientIds = appointments
                .Where(a => a.DoctorId == doctorId)
                .Select(a => a.PatientId)
                .ToHashSet();

            patients = patients.Where(p => patientIds.Contains(p.PatientId)).ToList();
        }

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
