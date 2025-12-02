using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SMMS.Application.Features.foodmenu.DTOs;
public sealed class MenuFoodItemInfo
{
    public int DailyMealId { get; set; }

    public int FoodId { get; set; }
    public string FoodName { get; set; } = default!;
    public string? FoodType { get; set; }
    public bool IsMainDish { get; set; }
    public string? ImageUrl { get; set; }
    public string? FoodDesc { get; set; }

    public int? SortOrder { get; set; }
}
