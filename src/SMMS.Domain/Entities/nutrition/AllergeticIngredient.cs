using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace SMMS.Domain.Entities.nutrition;

[PrimaryKey("IngredientId", "AllergenId")]
[Table("AllergeticIngredients", Schema = "nutrition")]
public partial class AllergeticIngredient
{
    [Key]
    public int IngredientId { get; set; }

    [Key]
    public int AllergenId { get; set; }

    public DateTime? ReportedAt { get; set; }

    [StringLength(10)]
    public string? SeverityLevel { get; set; }

    [StringLength(500)]
    public string? Notes { get; set; }

    [StringLength(500)]
    public string? ReactionNotes { get; set; }

    [StringLength(500)]
    public string? HandlingNotes { get; set; }

    public DateTime? DiagnosedAt { get; set; }

    [ForeignKey("AllergenId")]
    [InverseProperty("AllergeticIngredients")]
    public virtual Allergen Allergen { get; set; } = null!;

    [ForeignKey("IngredientId")]
    [InverseProperty("AllergeticIngredients")]
    public virtual Ingredient Ingredient { get; set; } = null!;
}
