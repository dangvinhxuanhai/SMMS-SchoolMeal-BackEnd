using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SMMS.Application.Features.nutrition.DTOs;
// Dùng trong body khi tạo/cập nhật FoodItem
public class FoodItemIngredientRequestDto
{
    public int IngredientId { get; set; }
    public decimal QuantityGram { get; set; }
}

// Dùng khi trả về cho client
public class FoodItemIngredientDto
{
    public int IngredientId { get; set; }
    public string IngredientName { get; set; } = string.Empty; // Thêm
    public decimal QuantityGram { get; set; }
    public string Unit { get; set; } = string.Empty; // Thêm
}
