using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SMMS.Application.Features.Meal.DTOs;
public class MenuDayDto
{
    public int MenuDayId { get; set; }
    public byte DayOfWeek { get; set; }     // 1..7
    public string MealType { get; set; } = null!;
    public string? Notes { get; set; }

    public List<MenuDayFoodItemDto> FoodItems { get; set; } = new();
}
