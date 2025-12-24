using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SMMS.Application.Features.Meal.DTOs;
using SMMS.Application.Features.Meal.Interfaces;
using SMMS.Persistence.Data;

namespace SMMS.Persistence.Repositories.Meal;
public class DailyMealRepository : IDailyMealRepository
{
    private readonly EduMealContext _context;

    public DailyMealRepository(EduMealContext context)
    {
        _context = context;
    }

    public async Task<DailyMealDetailPopupDto?> GetDailyMealDetailAsync(
        long dailyMealId,
        CancellationToken ct)
    {
        // ===== DailyMeal base info =====
        var dailyMeal = await _context.DailyMeals
            .AsNoTracking()
            .Where(dm => dm.DailyMealId == dailyMealId)
            .Select(dm => new DailyMealDetailPopupDto
            {
                DailyMealId = dm.DailyMealId,
                MealDate = dm.MealDate.ToDateTime(TimeOnly.MinValue),
                MealType = dm.MealType,
                Notes = dm.Notes
            })
            .FirstOrDefaultAsync(ct);

        if (dailyMeal == null)
            return null;

        // ===== Actual Ingredients =====
        dailyMeal.ActualIngredients = await (
            from ai in _context.DailyMealActualIngredients
            join ing in _context.Ingredients
                on ai.IngredientId equals ing.IngredientId
            where ai.DailyMealId == dailyMealId
            orderby ing.IngredientName
            select new DailyMealActualIngredientDto
            {
                IngredientId = ing.IngredientId,
                IngredientName = ing.IngredientName,
                ActualQtyGram = ai.ActualQtyGram
            }
        ).ToListAsync(ct);

        // ===== Evidences =====
        dailyMeal.Evidences = await _context.DailyMealEvidences
            .AsNoTracking()
            .Where(e => e.DailyMealId == dailyMealId)
            .OrderByDescending(e => e.UploadedAt)
            .Select(e => new DailyMealEvidenceDto
            {
                EvidenceId = e.EvidenceId,
                FileUrl = e.EvidenceUrl,
                UploadedAt = e.UploadedAt ?? DateTime.MinValue
            })
            .ToListAsync(ct);

        return dailyMeal;
    }
}
