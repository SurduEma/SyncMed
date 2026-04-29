using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SyncMed.Models;

public class Doctor
{
    [Key]
    [ForeignKey(nameof(User))]
    public int DoctorId { get; set; }

    public User User { get; set; } = null!;

    [Required, MaxLength(100)]
    public string Specialty { get; set; } = string.Empty;

    [Required, MaxLength(100)]
    public string DoctorLicenseId { get; set; } = string.Empty;

    [MaxLength(500)]
    public string? WorkingHours { get; set; }

    // Navigation properties
    public ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();
    public ICollection<Prescription> ApprovedPrescriptions { get; set; } = new List<Prescription>();
    public ICollection<TestResult> UploadedTestResults { get; set; } = new List<TestResult>();
}
