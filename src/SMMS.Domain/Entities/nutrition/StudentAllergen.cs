using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using SMMS.Domain.Entities.school;

namespace SMMS.Domain.Entities.nutrition;

[PrimaryKey("StudentId", "AllergenId")]
[Table("StudentAllergens", Schema = "nutrition")]
public partial class StudentAllergen
{
    [Key]
    public Guid StudentId { get; set; }

    [Key]
    public int AllergenId { get; set; }

    public DateTime? DiagnosedAt { get; set; }

    [StringLength(500)]
    public string? Notes { get; set; }

    [StringLength(500)]
    public string? ReactionNotes { get; set; }

    [StringLength(500)]
    public string? HandlingNotes { get; set; }

    [ForeignKey("AllergenId")]
    [InverseProperty("StudentAllergens")]
    public virtual Allergen Allergen { get; set; } = null!;

    [ForeignKey("StudentId")]
    [InverseProperty("StudentAllergens")]
    public virtual Student Student { get; set; } = null!;
}
