using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using SMMS.Domain.Entities.nutrition;

namespace SMMS.Domain.Entities.purchasing;

[PrimaryKey("PlanId", "IngredientId")]
[Table("PurchasePlanLines", Schema = "purchasing")]
public partial class PurchasePlanLine
{
    [Key]
    public int PlanId { get; set; }

    [Key]
    public int IngredientId { get; set; }

    [Column(TypeName = "decimal(12, 2)")]
    public decimal RqQuanityGram { get; set; }

    [ForeignKey("IngredientId")]
    [InverseProperty("PurchasePlanLines")]
    public virtual Ingredient Ingredient { get; set; } = null!;

    [ForeignKey("PlanId")]
    [InverseProperty("PurchasePlanLines")]
    public virtual PurchasePlan Plan { get; set; } = null!;
}
