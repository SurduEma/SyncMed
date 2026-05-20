using System.ComponentModel.DataAnnotations;

namespace SyncMed.Models;

public class MedicalService
{
    [Key]
    public int MedicalServiceId { get; set; }

    [Required, MaxLength(100)]
    public string Specialty { get; set; } = string.Empty;

    [Required, MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(1000)]
    public string? Description { get; set; }

    [Required]
    public decimal Price { get; set; }
}
