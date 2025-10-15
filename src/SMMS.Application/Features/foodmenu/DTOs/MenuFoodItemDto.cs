using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SMMS.Application.Features.foodmenu.DTOs;
public record MenuFoodItemDto(
    int FoodId,
    string FoodName,
    string? FoodType,
    string? ImageUrl,
    string? FoodDesc,
    bool IsAllergenRisk,                // true nếu có nguy cơ dị ứng với HS
    IReadOnlyList<string> MatchedAllergens // tên dị ứng khớp với món này
);
