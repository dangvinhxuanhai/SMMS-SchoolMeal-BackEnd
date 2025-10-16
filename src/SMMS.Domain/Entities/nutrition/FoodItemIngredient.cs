using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace SMMS.Domain.Entities.nutrition;

[PrimaryKey("FoodId", "IngredientId")]
[Table("FoodItemIngredients", Schema = "nutrition")]
public partial class FoodItemIngredient
{
    [Key]
    public int FoodId { get; set; }

    [Key]
    public int IngredientId { get; set; }

    [Column(TypeName = "decimal(9, 2)")]
    public decimal QuantityGram { get; set; }

    [ForeignKey("FoodId")]
    [InverseProperty("FoodItemIngredients")]
    public virtual FoodItem Food { get; set; } = null!;

    [ForeignKey("IngredientId")]
    [InverseProperty("FoodItemIngredients")]
    public virtual Ingredient Ingredient { get; set; } = null!;
}
