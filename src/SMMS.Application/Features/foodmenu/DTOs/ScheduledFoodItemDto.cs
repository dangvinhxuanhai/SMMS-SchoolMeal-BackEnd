using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SMMS.Application.Features.foodmenu.DTOs;
public sealed class ScheduledFoodItemDto
{
    public int FoodId { get; set; }
    public string FoodName { get; set; } = default!;
    public string? FoodType { get; set; }
    public bool IsMainDish { get; set; }
    public string? ImageUrl { get; set; }
    public string? FoodDesc { get; set; }

    /// <summary>
    /// Thứ tự hiển thị trong bữa ăn.
    /// </summary>
    public int? SortOrder { get; set; }

    // ✨ NEW: danh sách nguyên liệu và gram trong 1 món
    public IReadOnlyList<ScheduledFoodIngredientDto> Ingredients { get; set; }
        = Array.Empty<ScheduledFoodIngredientDto>();
}
