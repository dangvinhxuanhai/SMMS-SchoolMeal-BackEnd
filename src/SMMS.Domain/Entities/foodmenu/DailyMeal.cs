using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace SMMS.Domain.Entities.foodmenu;

[Table("DailyMeals", Schema = "foodmenu")]
[Index("ScheduleMealId", "MealDate", "MealType", Name = "UX_DailyMeals", IsUnique = true)]
public partial class DailyMeal
{
    [Key]
    public int DailyMealId { get; set; }

    public long ScheduleMealId { get; set; }

    public DateOnly MealDate { get; set; }

    [StringLength(20)]
    public string MealType { get; set; } = null!;

    [StringLength(300)]
    public string? Notes { get; set; }

    [InverseProperty("DailyMeal")]
    public virtual ICollection<Feedback> Feedbacks { get; set; } = new List<Feedback>();

    [InverseProperty("DailyMeal")]
    public virtual ICollection<MenuFoodItem> MenuFoodItems { get; set; } = new List<MenuFoodItem>();

    [ForeignKey("ScheduleMealId")]
    [InverseProperty("DailyMeals")]
    public virtual ScheduleMeal ScheduleMeal { get; set; } = null!;
}
