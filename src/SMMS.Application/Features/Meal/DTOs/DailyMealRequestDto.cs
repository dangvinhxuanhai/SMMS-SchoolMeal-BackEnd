using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SMMS.Application.Features.Meal.DTOs;
public class DailyMealRequestDto
{
    public DateTime MealDate { get; set; }         // ngày thực tế
    public string MealType { get; set; } = null!;  // "Breakfast", "Lunch", "Snack"...
    public string? Notes { get; set; }

    public List<int> FoodIds { get; set; } = new(); // các FoodId sẽ dùng trong bữa
}
