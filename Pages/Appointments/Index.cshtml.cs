using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Logging;
using SyncMed.Models;
using SyncMed.Services;

namespace SyncMed.Pages.Appointments;

public class IndexModel : PageModel
{
    private readonly IAppointmentService _appointmentService;
    private readonly IDoctorService _doctorService;
    private readonly IPatientService _patientService;
    private readonly ILogger<IndexModel> _logger;

    public IndexModel(
        IAppointmentService appointmentService,
        IDoctorService doctorService,
        IPatientService patientService,
        ILogger<IndexModel> logger)
    {
        _appointmentService = appointmentService;
        _doctorService = doctorService;
        _patientService = patientService;
        _logger = logger;
    }

    public IList<Appointment> Appointments { get; set; } = default!;

    public SelectList DoctorList { get; set; } = default!;

    public List<string> AvailableTimeSlots { get; set; } = new();

    [BindProperty]
    public int SelectedDoctorId { get; set; }

    [BindProperty]
    public DateOnly? AppointmentDate { get; set; }

    [BindProperty]
    public TimeOnly? AppointmentTime { get; set; }

    [BindProperty(SupportsGet = true)]
    public string? SearchDoctor { get; set; }

    [BindProperty(SupportsGet = true)]
    public string? SearchPatient { get; set; }

    [BindProperty(SupportsGet = true)]
    public DateOnly? SearchDate { get; set; }

    public bool HasSearched { get; set; }

    public async Task OnGetAsync()
    {
        await LoadDataAsync();
    }

    public async Task<JsonResult> OnGetTimeSlots(int doctorId, string? date)
    {
        var dateFormats = new[] { "yyyy-MM-dd", "dd-MM-yyyy" };
        if (doctorId == 0 || string.IsNullOrWhiteSpace(date))
        {
            _logger.LogWarning("Time slots request missing doctorId or date. doctorId={DoctorId}, date={Date}", doctorId, date);
            return new JsonResult(new List<string>());
        }

        if (!DateOnly.TryParseExact(date, dateFormats, System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None, out var parsedDate))
        {
            _logger.LogWarning("Time slots request has invalid date format. doctorId={DoctorId}, date={Date}", doctorId, date);
            return new JsonResult(new List<string>());
        }

        if (parsedDate < DateOnly.FromDateTime(DateTime.Today))
        {
            _logger.LogWarning("Time slots request date in the past. doctorId={DoctorId}, date={Date}", doctorId, parsedDate);
            return new JsonResult(new List<string>());
        }

        if (parsedDate.DayOfWeek is DayOfWeek.Saturday or DayOfWeek.Sunday)
        {
            _logger.LogWarning("Time slots request on weekend. doctorId={DoctorId}, date={Date}", doctorId, parsedDate);
            return new JsonResult(new List<string>());
        }

        var availableSlots = await _appointmentService.GetAvailableTimeSlotsAsync(doctorId, parsedDate);
        var formattedSlots = availableSlots.Select(t => t.ToString("HH:mm")).ToList();

        _logger.LogInformation("Time slots response. doctorId={DoctorId}, date={Date}, slots={Count}", doctorId, parsedDate, formattedSlots.Count);

        return new JsonResult(formattedSlots);
    }

    public async Task<IActionResult> OnPostAsync()
    {
        // Validate doctor selection
        if (SelectedDoctorId == 0)
        {
            ModelState.AddModelError(nameof(SelectedDoctorId), "Please select a doctor.");
        }

        // Validate appointment date
        if (!AppointmentDate.HasValue)
        {
            ModelState.AddModelError(nameof(AppointmentDate), "Please select an appointment date.");
        }
        else if (AppointmentDate.Value < DateOnly.FromDateTime(DateTime.Today))
        {
            ModelState.AddModelError(nameof(AppointmentDate), "Appointment date cannot be in the past.");
        }
        else if (AppointmentDate.Value.DayOfWeek is DayOfWeek.Saturday or DayOfWeek.Sunday)
        {
            ModelState.AddModelError(nameof(AppointmentDate), "Appointments cannot be booked on weekends.");
        }

        // Validate appointment time
        if (!AppointmentTime.HasValue)
        {
            ModelState.AddModelError(nameof(AppointmentTime), "Please select a time slot.");
        }

        if (!ModelState.IsValid)
        {
            await LoadDataAsync();
            return Page();
        }

        var availableSlots = await _appointmentService.GetAvailableTimeSlotsAsync(SelectedDoctorId, AppointmentDate.Value);
        if (!availableSlots.Contains(AppointmentTime.Value))
        {
            ModelState.AddModelError(nameof(AppointmentTime), "Selected time slot is not available.");
            await LoadDataAsync();
            return Page();
        }

        // Get the first patient (mocked current user)
        var patients = await _patientService.GetAllPatientsAsync();
        var patient = patients.FirstOrDefault();

        if (patient == null)
        {
            ModelState.AddModelError(string.Empty, "No patient found in the system.");
            await LoadDataAsync();
            return Page();
        }

        // Use the service to book the appointment
        var (success, message) = await _appointmentService.BookAppointmentAsync(
            patient.PatientId,
            SelectedDoctorId,
            AppointmentDate.Value,
            AppointmentTime.Value);

        if (!success)
        {
            ModelState.AddModelError(string.Empty, message);
            await LoadDataAsync();
            return Page();
        }

        return RedirectToPage();
    }

    private async Task LoadDataAsync() 
    {
        var allAppointments = await _appointmentService.GetAllAppointmentsAsync();

        if (!string.IsNullOrWhiteSpace(SearchDoctor))
        {
            var lowerDoctor = SearchDoctor.ToLowerInvariant();
            allAppointments = allAppointments
                .Where(a => a.Doctor?.User != null
                    && ($"{a.Doctor.User.FirstName} {a.Doctor.User.LastName}".ToLowerInvariant().Contains(lowerDoctor)
                        || (a.Doctor.User.FirstName?.ToLowerInvariant().Contains(lowerDoctor) ?? false)
                        || (a.Doctor.User.LastName?.ToLowerInvariant().Contains(lowerDoctor) ?? false)))
                .ToList();
            HasSearched = true;
        }

        if (!string.IsNullOrWhiteSpace(SearchPatient))
        {
            var lowerPatient = SearchPatient.ToLowerInvariant();
            allAppointments = allAppointments
                .Where(a => a.Patient?.User != null
                    && ($"{a.Patient.User.FirstName} {a.Patient.User.LastName}".ToLowerInvariant().Contains(lowerPatient)
                        || (a.Patient.User.FirstName?.ToLowerInvariant().Contains(lowerPatient) ?? false)
                        || (a.Patient.User.LastName?.ToLowerInvariant().Contains(lowerPatient) ?? false)))
                .ToList();
            HasSearched = true;
        }

        if (SearchDate.HasValue)
        {
            allAppointments = allAppointments
                .Where(a => a.AppointmentDate == SearchDate.Value)
                .ToList();
            HasSearched = true;
        }

        Appointments = allAppointments
            .OrderByDescending(a => a.AppointmentDate)
            .ThenByDescending(a => a.AppointmentTime)
            .ToList();

        var doctors = await _doctorService.GetAllDoctorsAsync();

        DoctorList = new SelectList(
            doctors.Select(d => new { d.DoctorId, Display = $"Dr. {d.User.FirstName} {d.User.LastName} — {d.Specialty}" }),
            "DoctorId",
            "Display");
    }
}
