using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace SMMS.Application.Features.nutrition.DTOs;
public class CreateFoodItemRequest
{
    public string FoodName { get; set; } = default!;
    public string? FoodType { get; set; }
    public string? FoodDesc { get; set; }
    public bool IsMainDish { get; set; } = true;

    // nếu bạn vẫn muốn hỗ trợ gửi URL trực tiếp
    public string? ImageUrl { get; set; }

    // 👇 file upload từ form
    public IFormFile? ImageFile { get; set; }

    public List<FoodItemIngredientRequestDto>? Ingredients { get; set; }
}
