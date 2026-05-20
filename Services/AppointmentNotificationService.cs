using SyncMed.Models;

namespace SyncMed.Services;

public interface IAppointmentNotificationService
{
    Task SendAppointmentConfirmationAsync(Appointment appointment);
}

public class AppointmentNotificationService : IAppointmentNotificationService
{
    private readonly ILogger<AppointmentNotificationService> _logger;

    public AppointmentNotificationService(ILogger<AppointmentNotificationService> logger)
    {
        _logger = logger;
    }

    public Task SendAppointmentConfirmationAsync(Appointment appointment)
    {
        var patientEmail = appointment.Patient?.User?.Email ?? "unknown patient";
        var doctorName = appointment.Doctor?.User == null
            ? "unknown doctor"
            : $"Dr. {appointment.Doctor.User.FirstName} {appointment.Doctor.User.LastName}";

        _logger.LogInformation(
            "Appointment confirmation queued for {PatientEmail}: {Date} at {Time} with {Doctor}.",
            patientEmail,
            appointment.AppointmentDate,
            appointment.AppointmentTime,
            doctorName);

        return Task.CompletedTask;
    }
}
