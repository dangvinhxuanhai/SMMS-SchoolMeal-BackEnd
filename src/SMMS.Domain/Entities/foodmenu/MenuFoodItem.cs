using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using SMMS.Domain.Entities.nutrition;

namespace SMMS.Domain.Entities.foodmenu;

[PrimaryKey("DailyMealId", "FoodId")]
[Table("MenuFoodItems", Schema = "foodmenu")]
public partial class MenuFoodItem
{
    [Key]
    public int DailyMealId { get; set; }

    [Key]
    public int FoodId { get; set; }

    public int? SortOrder { get; set; }

    [ForeignKey("DailyMealId")]
    [InverseProperty("MenuFoodItems")]
    public virtual DailyMeal DailyMeal { get; set; } = null!;

    [ForeignKey("FoodId")]
    [InverseProperty("MenuFoodItems")]
    public virtual FoodItem Food { get; set; } = null!;
}
