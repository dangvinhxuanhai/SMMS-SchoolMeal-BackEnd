using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SMMS.Application.Features.Meal.DTOs;
public class MenuDayFoodItemDto
{
    public int FoodId { get; set; }
    public int? SortOrder { get; set; }

    // Thông tin cơ bản của món ăn
    public string FoodName { get; set; } = string.Empty;
    public string? FoodType { get; set; }
    public string? FoodDesc { get; set; }
    public string? ImageUrl { get; set; }
    public bool IsMainDish { get; set; }
}
