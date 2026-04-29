using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SyncMed.Models;

public class Nurse
{
    [Key]
    [ForeignKey(nameof(User))]
    public int NurseId { get; set; }

    public User User { get; set; } = null!;

    [Required, MaxLength(100)]
    public string NurseLicenseId { get; set; } = string.Empty;

    // Navigation properties
    public ICollection<Prescription> DraftedPrescriptions { get; set; } = new List<Prescription>();
    public ICollection<Consultation> Consultations { get; set; } = new List<Consultation>();
}
