using Moq;
using SyncMed.Data.Repositories;
using SyncMed.Models;
using SyncMed.Services;

namespace SyncMed.Tests;

public class AppointmentServiceTests
{
    private readonly Mock<IAppointmentRepository> _apptRepo = new();
    private readonly Mock<IDoctorRepository> _doctorRepo = new();
    private readonly Mock<IPatientRepository> _patientRepo = new();
    private readonly Mock<IAppointmentNotificationService> _notificationService = new();
    private readonly AppointmentService _sut;

    public AppointmentServiceTests()
    {
        _sut = new AppointmentService(
            _apptRepo.Object, _doctorRepo.Object,
            _patientRepo.Object, _notificationService.Object);
    }

    private static DateOnly GetNextWeekday(DayOfWeek day)
    {
        var date = DateOnly.FromDateTime(DateTime.Today).AddDays(1);
        while (date.DayOfWeek != day) date = date.AddDays(1);
        return date;
    }

    private static DateOnly GetNextFutureWeekday()
    {
        var date = DateOnly.FromDateTime(DateTime.Today).AddDays(1);
        while (date.DayOfWeek is DayOfWeek.Saturday or DayOfWeek.Sunday) date = date.AddDays(1);
        return date;
    }

    private void SetupValidPatientAndDoctor(string? workingHours = null)
    {
        _patientRepo.Setup(r => r.GetByIdAsync(It.IsAny<int>()))
            .ReturnsAsync(new Patient { PatientId = 1, User = new User { FirstName = "John", LastName = "Doe" } });
        _doctorRepo.Setup(r => r.GetByIdAsync(It.IsAny<int>()))
            .ReturnsAsync(new Doctor
            {
                DoctorId = 1,
                Specialty = "Cardiology",
                DoctorLicenseId = "LIC-001",
                WorkingHours = workingHours,
                User = new User { FirstName = "Alice", LastName = "Smith" }
            });
        _apptRepo.Setup(r => r.GetByDoctorAndDateAsync(It.IsAny<int>(), It.IsAny<DateOnly>()))
            .ReturnsAsync(new List<Appointment>());
    }

    // --- ALGORITHM TEST 1: Weekend detection (Saturday) ---
    [Fact]
    public async Task BookAppointment_RejectsWeekend_Saturday()
    {
        var saturday = GetNextWeekday(DayOfWeek.Saturday);
        var (success, message) = await _sut.BookAppointmentAsync(1, 1, saturday, new TimeOnly(10, 0));
        Assert.False(success);
        Assert.Contains("weekends", message, StringComparison.OrdinalIgnoreCase);
    }

    // --- ALGORITHM TEST 2: Weekend detection (Sunday) ---
    [Fact]
    public async Task BookAppointment_RejectsWeekend_Sunday()
    {
        var sunday = GetNextWeekday(DayOfWeek.Sunday);
        var (success, message) = await _sut.BookAppointmentAsync(1, 1, sunday, new TimeOnly(10, 0));
        Assert.False(success);
        Assert.Contains("weekends", message, StringComparison.OrdinalIgnoreCase);
    }

    // --- ALGORITHM TEST 3: 30-minute interval validation ---
    [Fact]
    public async Task BookAppointment_RejectsNon30MinInterval()
    {
        var weekday = GetNextFutureWeekday();
        var (success, message) = await _sut.BookAppointmentAsync(1, 1, weekday, new TimeOnly(10, 15));
        Assert.False(success);
        Assert.Contains(":00 or :30", message);
    }

    // --- ALGORITHM TEST 4: Time slot conflict detection ---
    [Fact]
    public async Task BookAppointment_DetectsTimeSlotConflict()
    {
        var weekday = GetNextFutureWeekday();
        SetupValidPatientAndDoctor();

        _apptRepo.Setup(r => r.GetByDoctorAndDateAsync(1, weekday))
            .ReturnsAsync(new List<Appointment>
            {
                new() { AppointmentTime = new TimeOnly(10, 0), Status = "Pending" }
            });

        var (success, message) = await _sut.BookAppointmentAsync(1, 1, weekday, new TimeOnly(10, 0));
        Assert.False(success);
        Assert.Contains("overlaps", message, StringComparison.OrdinalIgnoreCase);
    }

    // --- ALGORITHM TEST 5: Working hours regex parsing + boundary check ---
    [Fact]
    public async Task BookAppointment_RejectsOutsideWorkingHours()
    {
        var weekday = GetNextFutureWeekday();
        SetupValidPatientAndDoctor("Mon-Fri 10:00 - 14:00");

        var (success, message) = await _sut.BookAppointmentAsync(1, 1, weekday, new TimeOnly(9, 0));
        Assert.False(success);
        Assert.Contains("10:00", message);
        Assert.Contains("14:00", message);
    }

    // --- ALGORITHM TEST 6: Available slot generation with conflict filtering ---
    [Fact]
    public async Task GetAvailableTimeSlots_FiltersBookedSlots()
    {
        var weekday = GetNextFutureWeekday();
        _doctorRepo.Setup(r => r.GetByIdAsync(1))
            .ReturnsAsync(new Doctor
            {
                DoctorId = 1, Specialty = "Cardiology",
                DoctorLicenseId = "LIC-001", WorkingHours = "09:00 - 11:00"
            });
        _apptRepo.Setup(r => r.GetByDoctorAndDateAsync(1, weekday))
            .ReturnsAsync(new List<Appointment>
            {
                new() { AppointmentTime = new TimeOnly(9, 30), Status = "Pending" }
            });

        var slots = await _sut.GetAvailableTimeSlotsAsync(1, weekday);

        Assert.Equal(3, slots.Count);
        Assert.Contains(new TimeOnly(9, 0), slots);
        Assert.Contains(new TimeOnly(10, 0), slots);
        Assert.Contains(new TimeOnly(10, 30), slots);
        Assert.DoesNotContain(new TimeOnly(9, 30), slots);
    }

    // --- BEHAVIOR TEST 7: Past date rejection ---
    [Fact]
    public async Task BookAppointment_RejectsPastDate()
    {
        var pastDate = DateOnly.FromDateTime(DateTime.Today).AddDays(-1);
        var (success, message) = await _sut.BookAppointmentAsync(1, 1, pastDate, new TimeOnly(10, 0));
        Assert.False(success);
        Assert.Contains("past", message, StringComparison.OrdinalIgnoreCase);
    }

    // --- BEHAVIOR TEST 8: Successful booking ---
    [Fact]
    public async Task BookAppointment_SucceedsWithValidInputs()
    {
        var weekday = GetNextFutureWeekday();
        SetupValidPatientAndDoctor();

        var (success, message) = await _sut.BookAppointmentAsync(1, 1, weekday, new TimeOnly(10, 0));

        Assert.True(success);
        _apptRepo.Verify(r => r.AddAsync(It.IsAny<Appointment>()), Times.Once);
        _notificationService.Verify(n => n.SendAppointmentConfirmationAsync(It.IsAny<Appointment>()), Times.Once);
    }

    // --- BEHAVIOR TEST 9: Cancel appointment ---
    [Fact]
    public async Task CancelAppointment_SetsCancelledStatus()
    {
        _apptRepo.Setup(r => r.GetByIdAsync(1))
            .ReturnsAsync(new Appointment { AppointmentId = 1, Status = "Pending" });

        var (success, _) = await _sut.CancelAppointmentAsync(1);

        Assert.True(success);
        _apptRepo.Verify(r => r.UpdateAsync(It.Is<Appointment>(a => a.Status == "Cancelled")), Times.Once);
    }

    // --- BEHAVIOR TEST 10: Already confirmed ---
    [Fact]
    public async Task ConfirmAppointment_AlreadyConfirmed_ReturnsFalse()
    {
        _apptRepo.Setup(r => r.GetByIdAsync(1))
            .ReturnsAsync(new Appointment { AppointmentId = 1, Status = "Confirmed" });

        var (success, message) = await _sut.ConfirmAppointmentAsync(1);

        Assert.False(success);
        Assert.Contains("already confirmed", message, StringComparison.OrdinalIgnoreCase);
    }
}
