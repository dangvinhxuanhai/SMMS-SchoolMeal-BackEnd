using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SMMS.Application.Features.Meal.DTOs;
public class DailyMealDetailPopupDto
{
    public long DailyMealId { get; set; }
    public DateTime MealDate { get; set; }
    public string MealType { get; set; } = null!;
    public string? Notes { get; set; }

    public List<DailyMealActualIngredientDto> ActualIngredients { get; set; } = new();
    public List<DailyMealEvidenceDto> Evidences { get; set; } = new();
}

public class DailyMealActualIngredientDto
{
    public int IngredientId { get; set; }
    public string IngredientName { get; set; } = null!;
    public decimal ActualQtyGram { get; set; }
}

public class DailyMealEvidenceDto
{
    public long EvidenceId { get; set; }
    public string FileUrl { get; set; } = null!;
    public DateTime UploadedAt { get; set; }
}
