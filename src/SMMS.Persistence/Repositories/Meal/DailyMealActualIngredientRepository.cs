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
public class DailyMealActualIngredientRepository
    : IDailyMealActualIngredientRepository
{
    private readonly EduMealContext _context;

    public DailyMealActualIngredientRepository(EduMealContext context)
    {
        _context = context;
    }

    public async Task BulkUpdateAsync(
    int dailyMealId,
    List<DailyMealActualIngredientUpdateDto> items,
    CancellationToken ct)
    {
        // Load kèm IngredientName để dùng cho message
        var entities = await (
            from ai in _context.DailyMealActualIngredients
            join ing in _context.Ingredients
                on ai.IngredientId equals ing.IngredientId
            where ai.DailyMealId == dailyMealId
            select new
            {
                Entity = ai,
                IngredientName = ing.IngredientName
            }
        ).ToListAsync(ct);

        foreach (var row in entities)
        {
            var entity = row.Entity;
            var ingredientName = row.IngredientName;

            var input = items
                .FirstOrDefault(x => x.IngredientId == entity.IngredientId);

            if (input == null)
                continue;

            // ===== VALIDATION NGHIỆP VỤ =====

            // Không cho âm
            if (input.ActualQtyGram < 0)
            {
                throw new InvalidOperationException(
                    $"Nguyên liệu \"{ingredientName}\": số lượng không hợp lệ.");
            }

            // Nếu tăng so với baseline → bắt buộc có lý do
            if (input.ActualQtyGram > entity.ActualQtyGram)
            {
                if (string.IsNullOrWhiteSpace(input.Notes))
                {
                    throw new InvalidOperationException(
                        $"Nguyên liệu \"{ingredientName}\": tăng định lượng phải nhập lý do.");
                }
            }

            // ===== UPDATE =====
            entity.ActualQtyGram = input.ActualQtyGram;
            entity.Notes = input.Notes;
        }

        await _context.SaveChangesAsync(ct);
    }

}
