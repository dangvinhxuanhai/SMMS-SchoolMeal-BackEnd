using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SMMS.Application.Features.nutrition.Interfaces;
using SMMS.Domain.Entities.nutrition;
using SMMS.Persistence.Data;
using SMMS.Persistence.Repositories.Skeleton;

namespace SMMS.Persistence.Repositories.nutrition;
public class FoodItemIngredientRepository : IFoodItemIngredientRepository
{
    private readonly EduMealContext _context;

    public FoodItemIngredientRepository(EduMealContext context)
    {
        _context = context;
    }

    private async Task MarkSchoolNeedRebuildAiIndexByFoodAsync(
    int foodId,
    CancellationToken ct)
    {
        var food = await _context.FoodItems
            .AsNoTracking()
            .FirstOrDefaultAsync(f => f.FoodId == foodId, ct);

        if (food == null)
            return;

        var school = await _context.Schools
            .FirstOrDefaultAsync(s => s.SchoolId == food.SchoolId, ct);

        if (school != null)
        {
            school.NeedRebuildAiIndex = false;
        }
    }

    public async Task<IReadOnlyList<FoodItemIngredient>> GetByFoodIdAsync(
        int foodId,
        CancellationToken cancellationToken = default)
    {
        return await _context.FoodItemIngredients
            .Where(x => x.FoodId == foodId)
            .ToListAsync(cancellationToken);
    }

    public async Task ReplaceForFoodAsync(
        int foodId,
        IEnumerable<FoodItemIngredient> newItems,
        CancellationToken cancellationToken = default)
    {
        // Xóa toàn bộ record cũ
        var existing = _context.FoodItemIngredients.Where(x => x.FoodId == foodId);
        _context.FoodItemIngredients.RemoveRange(existing);

        // Thêm record mới
        await _context.FoodItemIngredients.AddRangeAsync(newItems, cancellationToken);

        // Đánh dấu school cần rebuild AI index (theo foodId)
        await MarkSchoolNeedRebuildAiIndexByFoodAsync(foodId, cancellationToken);
    }
}
