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
    private readonly ILogger<EditModel> _logger;

    public EditModel(
        IAppointmentService appointmentService,
        IDoctorService doctorService,
        IPatientService patientService,
        ILogger<EditModel> logger)
    {
        _appointmentService = appointmentService;
        _doctorService = doctorService;
        _patientService = patientService;
        _logger = logger;
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

    public async Task<JsonResult> OnGetTimeSlots(int doctorId, string? date, int? appointmentId)
    {
        var dateFormats = new[] { "yyyy-MM-dd", "dd-MM-yyyy" };
        if (doctorId == 0 || string.IsNullOrWhiteSpace(date))
        {
            _logger.LogWarning("Edit time slots request missing doctorId or date. doctorId={DoctorId}, date={Date}", doctorId, date);
            return new JsonResult(new List<string>());
        }

        if (!DateOnly.TryParseExact(date, dateFormats, System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None, out var parsedDate))
        {
            _logger.LogWarning("Edit time slots request has invalid date format. doctorId={DoctorId}, date={Date}", doctorId, date);
            return new JsonResult(new List<string>());
        }

        if (parsedDate < DateOnly.FromDateTime(DateTime.Today))
        {
            _logger.LogWarning("Edit time slots request date in the past. doctorId={DoctorId}, date={Date}", doctorId, parsedDate);
            return new JsonResult(new List<string>());
        }

        if (parsedDate.DayOfWeek is DayOfWeek.Saturday or DayOfWeek.Sunday)
        {
            _logger.LogWarning("Edit time slots request on weekend. doctorId={DoctorId}, date={Date}", doctorId, parsedDate);
            return new JsonResult(new List<string>());
        }

        var availableSlots = await _appointmentService.GetAvailableTimeSlotsAsync(doctorId, parsedDate);

        if (appointmentId.HasValue)
        {
            var existing = await _appointmentService.GetAppointmentByIdAsync(appointmentId.Value);
            if (existing != null
                && existing.DoctorId == doctorId
                && existing.AppointmentDate == parsedDate
                && !availableSlots.Contains(existing.AppointmentTime))
            {
                availableSlots.Add(existing.AppointmentTime);
                availableSlots = availableSlots.OrderBy(slot => slot).ToList();
            }
        }

        var formattedSlots = availableSlots.Select(t => t.ToString("HH:mm")).ToList();

        _logger.LogInformation("Edit time slots response. doctorId={DoctorId}, date={Date}, slots={Count}", doctorId, parsedDate, formattedSlots.Count);

        return new JsonResult(formattedSlots);
    }

    public async Task<IActionResult> OnPostAsync()
    {
        _logger.LogInformation("Edit appointment POST received. AppointmentId: {AppointmentId}", Appointment?.AppointmentId);

        if (Appointment == null)
        {
            ModelState.AddModelError(string.Empty, "Appointment data is missing.");
            await LoadDropdownsAsync();
            return Page();
        }

        var existing = await _appointmentService.GetAppointmentByIdAsync(Appointment.AppointmentId);
        if (existing == null)
        {
            _logger.LogWarning("Edit appointment failed. Appointment {AppointmentId} not found.", Appointment.AppointmentId);
            ModelState.AddModelError(string.Empty, "Appointment not found.");
            await LoadDropdownsAsync();
            return Page();
        }

        if (Appointment.PatientId == 0)
        {
            ModelState.AddModelError("Appointment.PatientId", "Please select a patient.");
        }

        if (Appointment.DoctorId == 0)
        {
            ModelState.AddModelError("Appointment.DoctorId", "Please select a doctor.");
        }

        if (Appointment.AppointmentDate == default)
        {
            ModelState.AddModelError("Appointment.AppointmentDate", "Please select an appointment date.");
        }
        else if (Appointment.AppointmentDate < DateOnly.FromDateTime(DateTime.Today))
        {
            ModelState.AddModelError("Appointment.AppointmentDate", "Appointment date cannot be in the past.");
        }
        else if (Appointment.AppointmentDate.DayOfWeek is DayOfWeek.Saturday or DayOfWeek.Sunday)
        {
            ModelState.AddModelError("Appointment.AppointmentDate", "Appointments cannot be booked on weekends.");
        }

        if (Appointment.AppointmentTime == default)
        {
            ModelState.AddModelError("Appointment.AppointmentTime", "Please select a time slot.");
        }
        else if (Appointment.AppointmentTime.Minute is not (0 or 30))
        {
            ModelState.AddModelError("Appointment.AppointmentTime", "Appointments must start at :00 or :30 minutes.");
        }

        ModelState.Remove("Appointment.Patient");
        ModelState.Remove("Appointment.Doctor");
        ModelState.Remove("Appointment.Consultation");

        if (!ModelState.IsValid)
        {
            var errors = ModelState.Values
                .SelectMany(v => v.Errors)
                .Select(e => e.ErrorMessage)
                .ToArray();
            _logger.LogWarning("Edit appointment validation failed. Errors: {Errors}", string.Join(" | ", errors));
            await LoadDropdownsAsync();
            return Page();
        }

        try
        {
            var availableSlots = await _appointmentService.GetAvailableTimeSlotsAsync(Appointment.DoctorId, Appointment.AppointmentDate);
            var isSameSlot = existing.DoctorId == Appointment.DoctorId
                && existing.AppointmentDate == Appointment.AppointmentDate
                && existing.AppointmentTime == Appointment.AppointmentTime;

            if (!isSameSlot && !availableSlots.Contains(Appointment.AppointmentTime))
            {
                ModelState.AddModelError("Appointment.AppointmentTime", "Selected time slot is not available.");
                await LoadDropdownsAsync();
                return Page();
            }

            await _appointmentService.UpdateAppointmentAsync(Appointment);
            _logger.LogInformation("Edit appointment succeeded. AppointmentId: {AppointmentId}", Appointment.AppointmentId);
            return RedirectToPage("Index");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Edit appointment failed. AppointmentId: {AppointmentId}", Appointment?.AppointmentId);
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
