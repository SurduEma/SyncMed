using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SyncMed.Authorization;
using SyncMed.Extensions;
using SyncMed.Models;
using SyncMed.Services;

namespace SyncMed.Pages.Consultations;

[Authorize(Roles = AppRoles.DoctorOrAdmin)]
public class NotesModel : PageModel
{
    private readonly IAppointmentService _appointmentService;
    private readonly IClinicalRecordService _clinicalRecordService;

    public NotesModel(IAppointmentService appointmentService, IClinicalRecordService clinicalRecordService)
    {
        _appointmentService = appointmentService;
        _clinicalRecordService = clinicalRecordService;
    }

    [BindProperty(SupportsGet = true)]
    public int AppointmentId { get; set; }

    [BindProperty, MaxLength(2000)]
    public string? Symptoms { get; set; }

    [BindProperty, MaxLength(2000)]
    public string? Diagnosis { get; set; }

    public Appointment Appointment { get; set; } = default!;
    public string? Vitals { get; set; }

    public async Task<IActionResult> OnGetAsync()
    {
        var result = await LoadAppointmentAsync();
        if (result != null)
            return result;

        var consultation = await _clinicalRecordService.GetConsultationByAppointmentIdAsync(AppointmentId);
        Vitals = consultation?.Vitals;
        Symptoms = consultation?.Symptoms;
        Diagnosis = consultation?.Diagnosis;

        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        var result = await LoadAppointmentAsync();
        if (result != null)
            return result;

        if (!ModelState.IsValid)
            return Page();

        var saveResult = await _clinicalRecordService.SaveDoctorNotesAsync(AppointmentId, Symptoms, Diagnosis);
        if (!saveResult.Success)
        {
            ModelState.AddModelError(string.Empty, saveResult.Message);
            return Page();
        }

        return RedirectToPage("/Patients/Details", new { id = Appointment.PatientId });
    }

    private async Task<IActionResult?> LoadAppointmentAsync()
    {
        var appointment = await _appointmentService.GetAppointmentByIdAsync(AppointmentId);
        if (appointment == null)
            return NotFound();

        if (User.IsInRole(AppRoles.Doctor) && appointment.DoctorId != User.GetUserId())
            return Forbid();

        Appointment = appointment;
        return null;
    }
}
