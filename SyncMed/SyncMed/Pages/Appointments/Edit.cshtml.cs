using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using SyncMed.Models;
using SyncMed.Services;

namespace SyncMed.Pages.Appointments;

public class EditModel : PageModel
{
    private readonly IAppointmentService _appointmentService;
    private readonly IDoctorService _doctorService;
    private readonly IPatientService _patientService;

    public EditModel(
        IAppointmentService appointmentService,
        IDoctorService doctorService,
        IPatientService patientService)
    {
        _appointmentService = appointmentService;
        _doctorService = doctorService;
        _patientService = patientService;
    }

    [BindProperty]
    public Appointment Appointment { get; set; } = default!;

    public SelectList DoctorList { get; set; } = default!;
    public SelectList PatientList { get; set; } = default!;
    public SelectList StatusList { get; set; } = default!;

    public async Task<IActionResult> OnGetAsync(int id)
    {
        Appointment = await _appointmentService.GetAppointmentByIdAsync(id);
        if (Appointment == null)
            return NotFound();

        await LoadDropdownsAsync();
        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            await LoadDropdownsAsync();
            return Page();
        }

        try
        {
            // Validate the appointment still exists
            var existing = await _appointmentService.GetAppointmentByIdAsync(Appointment.AppointmentId);
            if (existing == null)
            {
                ModelState.AddModelError(string.Empty, "Appointment not found.");
                await LoadDropdownsAsync();
                return Page();
            }

            await _appointmentService.UpdateAppointmentAsync(Appointment);
            return RedirectToPage("Index");
        }
        catch (Exception ex)
        {
            ModelState.AddModelError(string.Empty, $"Error: {ex.Message}");
            await LoadDropdownsAsync();
            return Page();
        }
    }

    private async Task LoadDropdownsAsync()
    {
        var doctors = await _doctorService.GetAllDoctorsAsync();
        DoctorList = new SelectList(
            doctors.Select(d => new { d.DoctorId, Display = $"Dr. {d.User.FirstName} {d.User.LastName} — {d.Specialty}" }),
            "DoctorId",
            "Display",
            Appointment.DoctorId);

        var patients = await _patientService.GetAllPatientsAsync();
        PatientList = new SelectList(
            patients.Select(p => new { p.PatientId, Display = $"{p.User.FirstName} {p.User.LastName}" }),
            "PatientId",
            "Display",
            Appointment.PatientId);

        var statuses = new[] { "Pending", "Confirmed", "Cancelled", "Completed" };
        StatusList = new SelectList(statuses, Appointment.Status);
    }
}
