using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SyncMed.Models;

public class Consultation
{
    [Key]
    public int ConsultationId { get; set; }

    public int AppointmentId { get; set; }

    [ForeignKey(nameof(AppointmentId))]
    public Appointment Appointment { get; set; } = null!;

    public int NurseId { get; set; }

    [ForeignKey(nameof(NurseId))]
    public Nurse Nurse { get; set; } = null!;

    [MaxLength(1000)]
    public string? Vitals { get; set; }

    [MaxLength(2000)]
    public string? Symptoms { get; set; }

    [MaxLength(2000)]
    public string? Diagnosis { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
