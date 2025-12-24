using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SMMS.Application.Features.Meal.DTOs;

namespace SMMS.Application.Features.Meal.Interfaces;
public interface IDailyMealActualIngredientRepository
{
    Task BulkUpdateAsync(
        int dailyMealId,
        List<DailyMealActualIngredientUpdateDto> items,
        CancellationToken ct);
}
