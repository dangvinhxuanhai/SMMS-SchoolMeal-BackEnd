using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SMMS.Application.Features.foodmenu.DTOs;
using SMMS.Application.Features.foodmenu.Interfaces;
using SMMS.Domain.Entities.foodmenu;
using SMMS.Persistence.Data;
using SMMS.Persistence.Repositories.Skeleton;

namespace SMMS.Persistence.Repositories.foodmenu;
public class ScheduleMealRepository : IScheduleMealRepository
{
    private readonly EduMealContext _context;

    public ScheduleMealRepository(EduMealContext context)
    {
        _context = context;
    }

    public Task<int> CountBySchoolAsync(Guid schoolId, CancellationToken ct = default)
    {
        return _context.ScheduleMeals
            .Where(x => x.SchoolId == schoolId)
            .CountAsync(ct);
    }
    public async Task<IReadOnlyList<ScheduleMeal>> GetAllBySchoolAsync(
        Guid schoolId,
        CancellationToken ct = default)
    {
        return await _context.ScheduleMeals
            .Where(x => x.SchoolId == schoolId)
            .OrderByDescending(x => x.WeekStart)
            .ToListAsync(ct);
    }

    public async Task<IReadOnlyList<ScheduleMeal>> GetPagedBySchoolAsync(
        Guid schoolId,
        int pageIndex,
        int pageSize,
        CancellationToken ct = default)
    {
        var skip = (pageIndex - 1) * pageSize;

        return await _context.ScheduleMeals
            .Where(x => x.SchoolId == schoolId)
            .OrderByDescending(x => x.WeekStart) // tuần mới nhất trước
            .Skip(skip)
            .Take(pageSize)
            .ToListAsync(ct);
    }

    public Task<ScheduleMeal?> GetForDateAsync(
    Guid schoolId,
    DateTime date,
    CancellationToken ct = default)
    {
        var d = DateOnly.FromDateTime(date);

        return _context.ScheduleMeals
            .FirstOrDefaultAsync(
                x => x.SchoolId == schoolId
                  && x.WeekStart <= d
                  && x.WeekEnd >= d,
                ct);
    }

    public async Task<IReadOnlyList<DailyMeal>> GetDailyMealsForSchedulesAsync(
        IEnumerable<long> scheduleMealIds,
        CancellationToken ct = default)
    {
        var ids = scheduleMealIds.ToList();
        if (ids.Count == 0)
            return Array.Empty<DailyMeal>();

        return await _context.DailyMeals
            .Where(d => ids.Contains(d.ScheduleMealId))
            .OrderBy(d => d.MealDate)
            .ThenBy(d => d.MealType)
            .ToListAsync(ct);
    }

    public async Task<IReadOnlyList<DailyMeal>> GetDailyMealsForScheduleAsync(
        long scheduleMealId,
        CancellationToken ct = default)
    {
        return await _context.DailyMeals
            .Where(d => d.ScheduleMealId == scheduleMealId)
            .OrderBy(d => d.MealDate)
            .ThenBy(d => d.MealType)
            .ToListAsync(ct)!;
    }

    public async Task<ScheduleMeal?> FindBySchoolAndWeekAsync(
    Guid schoolId,
    DateTime weekStart,
    CancellationToken ct = default)
    {
        var weekStartDateOnly = DateOnly.FromDateTime(weekStart);
        return await _context.ScheduleMeals
            .Include(s => s.DailyMeals)
                .ThenInclude(d => d.MenuFoodItems)
            .FirstOrDefaultAsync(s => s.SchoolId == schoolId
                                   && s.WeekStart == weekStartDateOnly, ct);
    }

    public async Task AddAsync(ScheduleMeal scheduleMeal, CancellationToken ct = default)
    {
        await _context.ScheduleMeals.AddAsync(scheduleMeal, ct);
    }

    public async Task<IReadOnlyList<MenuFoodItemInfo>> GetMenuFoodItemsForDailyMealsAsync(
    IEnumerable<int> dailyMealIds,
    CancellationToken ct = default)
    {
        var ids = dailyMealIds.Distinct().ToList();
        if (ids.Count == 0)
            return Array.Empty<MenuFoodItemInfo>();

        return await _context.MenuFoodItems
            .Where(mf => ids.Contains(mf.DailyMealId))
            .Select(mf => new MenuFoodItemInfo
            {
                DailyMealId = mf.DailyMealId,
                FoodId = mf.FoodId,
                SortOrder = mf.SortOrder,

                FoodName = mf.Food.FoodName,
                FoodType = mf.Food.FoodType,
                ImageUrl = mf.Food.ImageUrl,
                FoodDesc = mf.Food.FoodDesc,
                IsMainDish = mf.Food.IsMainDish
            })
            .OrderBy(x => x.DailyMealId)
            .ThenBy(x => x.SortOrder ?? int.MaxValue)
            .ToListAsync(ct);
    }

    public Task<ScheduleMeal?> GetByIdAsync(
        long scheduleMealId,
        CancellationToken ct = default)
    {
        return _context.ScheduleMeals
            .FirstOrDefaultAsync(x => x.ScheduleMealId == scheduleMealId, ct);
        // Nếu sau này cần DailyMeals thì có thể đổi thành:
        // return _context.ScheduleMeals
        //     .Include(s => s.DailyMeals)
        //     .FirstOrDefaultAsync(x => x.ScheduleMealId == scheduleMealId, ct);
    }

    public async Task<IReadOnlyList<FoodIngredientInfo>> GetFoodIngredientsForFoodsAsync(
    IEnumerable<int> foodIds,
    CancellationToken ct = default)
    {
        var ids = foodIds.Distinct().ToList();
        if (ids.Count == 0)
            return Array.Empty<FoodIngredientInfo>();

        return await _context.FoodItemIngredients
            .Where(fi => ids.Contains(fi.FoodId))
            .Select(fi => new FoodIngredientInfo
            {
                FoodId = fi.FoodId,
                IngredientId = fi.IngredientId,
                IngredientName = fi.Ingredient.IngredientName,
                QuantityGram = fi.QuantityGram
            })
            .OrderBy(x => x.FoodId)
            .ThenBy(x => x.IngredientName)
            .ToListAsync(ct);
    }
}
