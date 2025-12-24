using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SMMS.Application.Features.Meal.DTOs;
public class UpdateDailyMealActualIngredientsRequest
{
    public List<DailyMealActualIngredientUpdateDto> Items { get; set; } = new();
}

public class DailyMealActualIngredientUpdateDto
{
    public int IngredientId { get; set; }
    public decimal ActualQtyGram { get; set; }
    public string? Notes { get; set; }
}
