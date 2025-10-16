using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using SMMS.Domain.Entities.auth;

namespace SMMS.Domain.Entities.nutrition;

[PrimaryKey("IngredientId", "AltIngredientId")]
[Table("IngredientAlternatives", Schema = "nutrition")]
public partial class IngredientAlternative
{
    [Key]
    public int AltIngredientId { get; set; }

    [Key]
    public int IngredientId { get; set; }

    [StringLength(30)]
    public string? ReasonCode { get; set; }

    [Column(TypeName = "decimal(4, 3)")]
    public decimal? ConfidenceScore { get; set; }

    [StringLength(20)]
    public string SourceType { get; set; } = null!;

    public DateTime CreatedAt { get; set; }

    public Guid? CreatedBy { get; set; }

    [ForeignKey("AltIngredientId")]
    [InverseProperty("IngredientAlternativeAltIngredients")]
    public virtual Ingredient AltIngredient { get; set; } = null!;

    [ForeignKey("CreatedBy")]
    [InverseProperty("IngredientAlternatives")]
    public virtual User? CreatedByNavigation { get; set; }

    [ForeignKey("IngredientId")]
    [InverseProperty("IngredientAlternativeIngredients")]
    public virtual Ingredient Ingredient { get; set; } = null!;
}
