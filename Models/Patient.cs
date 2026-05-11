using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SyncMed.Models;

public class Patient
{
    [Key]
    [ForeignKey(nameof(User))]
    public int PatientId { get; set; }

    public User User { get; set; } = null!;

    public DateOnly DateOfBirth { get; set; }

    [MaxLength(20)]
    public string? PhoneNumber { get; set; }

    // Navigation properties
    public ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();
    public ICollection<Prescription> Prescriptions { get; set; } = new List<Prescription>();
    public ICollection<TestResult> TestResults { get; set; } = new List<TestResult>();
}
