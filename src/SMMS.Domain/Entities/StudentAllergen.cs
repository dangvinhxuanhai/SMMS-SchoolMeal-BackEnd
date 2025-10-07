using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace SMMS.Domain.Entities;

[PrimaryKey("StudentId", "AllergenId")]
public partial class StudentAllergen
{
    [Key]
    public int StudentId { get; set; }

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
