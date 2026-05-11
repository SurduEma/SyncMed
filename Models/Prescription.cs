using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SyncMed.Models;

public class Prescription
{
    [Key]
    public int PrescriptionId { get; set; }

    public int PatientId { get; set; }

    [ForeignKey(nameof(PatientId))]
    public Patient Patient { get; set; } = null!;

    public int DoctorId { get; set; }

    [ForeignKey(nameof(DoctorId))]
    public Doctor Doctor { get; set; } = null!;

    public int? DraftedByNurseId { get; set; }

    [ForeignKey(nameof(DraftedByNurseId))]
    public Nurse? DraftedByNurse { get; set; }

    [Required, MaxLength(4000)]
    public string MedicationDetails { get; set; } = string.Empty;

    [Required, MaxLength(50)]
    public string Status { get; set; } = string.Empty;

    public DateTime DateIssued { get; set; }
}
