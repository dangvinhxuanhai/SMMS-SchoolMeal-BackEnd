using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace SMMS.Application.Features.foodmenu.DTOs;
public class AiDishDto
{
    [JsonPropertyName("food_id")]
    public int FoodId { get; set; }

    [JsonPropertyName("food_name")]
    public string FoodName { get; set; } = default!;

    [JsonPropertyName("is_main_dish")]
    public bool IsMainDish { get; set; }

    [JsonPropertyName("total_kcal")]
    public double? TotalKcal { get; set; }

    [JsonPropertyName("score")]
    public double Score { get; set; }
}

// 2) Raw response từ Python (KHÔNG có SessionId)
public class AiMenuRawResponse
{
    [JsonPropertyName("recommended_main")]
    public List<AiDishDto> RecommendedMain { get; set; } = new();

    [JsonPropertyName("recommended_side")]
    public List<AiDishDto> RecommendedSide { get; set; } = new();
}

// 3) Response trả về FE (CÓ SessionId do .NET sinh ra)
public class AiMenuRecommendResponse
{
    public long SessionId { get; set; }
    public List<AiDishDto> RecommendedMain { get; set; } = new();
    public List<AiDishDto> RecommendedSide { get; set; } = new();
}

// món user thực sự chọn (dùng cho API log selection)
public class SelectedDishDto
{
    public int FoodId { get; set; }
    public bool IsMain { get; set; }
}

// request gửi sang Python
public class AiMenuRecommendRequest
{
    [JsonPropertyName("user_id")]
    public Guid UserId { get; set; }

    [JsonPropertyName("school_id")]
    public Guid SchoolId { get; set; }

    [JsonPropertyName("main_ingredient_ids")]
    public List<int> MainIngredientIds { get; set; } = new();

    [JsonPropertyName("side_ingredient_ids")]
    public List<int> SideIngredientIds { get; set; } = new();

    [JsonPropertyName("avoid_allergen_ids")]
    public List<int> AvoidAllergenIds { get; set; } = new();

    [JsonPropertyName("max_main_kcal")]
    public double? MaxMainKcal { get; set; } = 600;

    [JsonPropertyName("max_side_kcal")]
    public double? MaxSideKcal { get; set; } = 250;

    [JsonPropertyName("top_k_main")]
    public int TopKMain { get; set; } = 5;

    [JsonPropertyName("top_k_side")]
    public int TopKSide { get; set; } = 5;
}
