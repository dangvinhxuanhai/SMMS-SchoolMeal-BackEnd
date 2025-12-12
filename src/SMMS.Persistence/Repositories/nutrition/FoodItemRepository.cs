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
public class FoodItemRepository : IFoodItemRepository
{
    private readonly EduMealContext _context;

    public FoodItemRepository(EduMealContext context)
    {
        _context = context;
    }

    private async Task MarkSchoolNeedRebuildAiIndexAsync(Guid schoolId, CancellationToken ct)
    {
        var school = await _context.Schools
            .FirstOrDefaultAsync(s => s.SchoolId == schoolId, ct);

        if (school != null)
        {
            school.NeedRebuildAiIndex = false;
        }
    }

    public Task<FoodItem?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return _context.FoodItems
            .FirstOrDefaultAsync(x => x.FoodId == id, cancellationToken);
    }

    public async Task<IReadOnlyList<FoodItem>> GetListAsync(
        Guid schoolId,
        string? keyword,
        bool includeInactive = false,
        CancellationToken cancellationToken = default)
    {
        var query = _context.FoodItems
            .Where(x => x.SchoolId == schoolId);

        if (!includeInactive)
        {
            query = query.Where(x => x.IsActive);
        }

        if (!string.IsNullOrWhiteSpace(keyword))
        {
            query = query.Where(x => x.FoodName.Contains(keyword));
        }

        return await query
            .OrderBy(x => x.FoodName)
            .ToListAsync(cancellationToken);
    }


    public async Task AddAsync(FoodItem entity, CancellationToken cancellationToken = default)
    {
        await _context.FoodItems.AddAsync(entity, cancellationToken);
        await MarkSchoolNeedRebuildAiIndexAsync(entity.SchoolId, cancellationToken);
    }

    public async Task UpdateAsync(FoodItem entity, CancellationToken cancellationToken = default)
    {
        _context.FoodItems.Update(entity);
        await MarkSchoolNeedRebuildAiIndexAsync(entity.SchoolId, cancellationToken);
    }

    public async Task SoftDeleteAsync(FoodItem entity, CancellationToken cancellationToken = default)
    {
        entity.IsActive = false;
        _context.FoodItems.Update(entity);
        await MarkSchoolNeedRebuildAiIndexAsync(entity.SchoolId, cancellationToken);
    }

    public async Task<bool> HasRelationsAsync(int foodId, CancellationToken cancellationToken = default)
    {
        // check các bảng FK tới FoodItems
        var hasFoodItemIngredients = await _context.FoodItemIngredients
            .AnyAsync(x => x.FoodId == foodId, cancellationToken);

        var hasMenuDayFoodItems = await _context.MenuDayFoodItems
            .AnyAsync(x => x.FoodId == foodId, cancellationToken);

        var hasMenuFoodItems = await _context.MenuFoodItems
            .AnyAsync(x => x.FoodId == foodId, cancellationToken);

        var hasFoodInFridge = await _context.FoodInFridges
            .AnyAsync(x => x.FoodId == foodId && !x.IsDeleted, cancellationToken);

        var hasRecommendResults = await _context.MenuRecommendResults
            .AnyAsync(x => x.FoodId == foodId, cancellationToken);

        return hasFoodItemIngredients
               || hasMenuDayFoodItems
               || hasMenuFoodItems
               || hasFoodInFridge
               || hasRecommendResults;
    }

    public async Task HardDeleteAsync(FoodItem entity, CancellationToken cancellationToken = default)
    {
        var foodId = entity.FoodId;

        // 1. Xóa các quan hệ N-N
        var itemIngredients = _context.FoodItemIngredients.Where(x => x.FoodId == foodId);
        _context.FoodItemIngredients.RemoveRange(itemIngredients);

        var menuDayItems = _context.MenuDayFoodItems.Where(x => x.FoodId == foodId);
        _context.MenuDayFoodItems.RemoveRange(menuDayItems);

        var menuFoodItems = _context.MenuFoodItems.Where(x => x.FoodId == foodId);
        _context.MenuFoodItems.RemoveRange(menuFoodItems);

        // 2. Xử lý các bảng “transactional” (FoodInFridge, MenuRecommendResults)
        var foodInFridge = _context.FoodInFridges.Where(x => x.FoodId == foodId);
        // thường chỗ này nên SoftDelete hoặc cấm hard-delete món nếu còn mẫu đông lạnh
        _context.FoodInFridges.RemoveRange(foodInFridge);

        var recommendResults = _context.MenuRecommendResults.Where(x => x.FoodId == foodId);
        _context.MenuRecommendResults.RemoveRange(recommendResults);

        // 3. Cuối cùng mới xóa chính FoodItem
        _context.FoodItems.Remove(entity);
        await MarkSchoolNeedRebuildAiIndexAsync(entity.SchoolId, cancellationToken);
    }
}
