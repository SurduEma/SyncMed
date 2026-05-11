using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SyncMed.Models;

public class TestResult
{
    [Key]
    public int TestId { get; set; }

    public int PatientId { get; set; }

    [ForeignKey(nameof(PatientId))]
    public Patient Patient { get; set; } = null!;

    public int UploadedByDoctorId { get; set; }

    [ForeignKey(nameof(UploadedByDoctorId))]
    public Doctor UploadedByDoctor { get; set; } = null!;

    [Required, MaxLength(200)]
    public string TestName { get; set; } = string.Empty;

    [Required, MaxLength(500)]
    public string DocumentUrl { get; set; } = string.Empty;

    public DateTime DateUploaded { get; set; }
}
