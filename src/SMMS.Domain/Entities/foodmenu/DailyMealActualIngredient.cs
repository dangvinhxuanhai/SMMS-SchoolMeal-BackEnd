using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using SMMS.Domain.Entities.nutrition;

namespace SMMS.Domain.Entities.foodmenu;

[Table("DailyMealActualIngredients", Schema = "foodmenu")]
[Index("DailyMealId", Name = "IX_ActualIngredients_Meal")]
public partial class DailyMealActualIngredient
{
    [Key]
    public long ActualId { get; set; }

    public int DailyMealId { get; set; }

    public int IngredientId { get; set; }

    [Column(TypeName = "decimal(12, 2)")]
    public decimal ActualQtyGram { get; set; }

    [StringLength(255)]
    public string? Notes { get; set; }

    public DateTime? CreatedAt { get; set; }

    [ForeignKey("DailyMealId")]
    [InverseProperty("DailyMealActualIngredients")]
    public virtual DailyMeal DailyMeal { get; set; } = null!;

    [ForeignKey("IngredientId")]
    [InverseProperty("DailyMealActualIngredients")]
    public virtual Ingredient Ingredient { get; set; } = null!;
}
