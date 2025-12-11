using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SMMS.Application.Features.foodmenu.DTOs;
using SMMS.Application.Features.Skeleton.Interfaces;
using SMMS.Domain.Entities.foodmenu;

namespace SMMS.Application.Features.foodmenu.Interfaces;
public interface IScheduleMealRepository
{
    Task<int> CountBySchoolAsync(Guid schoolId, CancellationToken ct = default);

    Task<IReadOnlyList<ScheduleMeal>> GetPagedBySchoolAsync(
        Guid schoolId,
        int pageIndex,
        int pageSize,
        CancellationToken ct = default);
    Task<IReadOnlyList<ScheduleMeal>> GetAllBySchoolAsync(
        Guid schoolId,
        CancellationToken ct = default);
    Task<ScheduleMeal?> GetForDateAsync(
        Guid schoolId,
        DateTime date,
        CancellationToken ct = default);

    Task<IReadOnlyList<DailyMeal>> GetDailyMealsForSchedulesAsync(
        IEnumerable<long> scheduleMealIds,
        CancellationToken ct = default);

    Task<IReadOnlyList<DailyMeal>> GetDailyMealsForScheduleAsync(
        long scheduleMealId,
        CancellationToken ct = default);

    Task<ScheduleMeal?> FindBySchoolAndWeekAsync(
    Guid schoolId,
    DateTime weekStart,
    CancellationToken ct = default);

    Task AddAsync(ScheduleMeal scheduleMeal, CancellationToken ct = default);
    Task<IReadOnlyList<MenuFoodItemInfo>> GetMenuFoodItemsForDailyMealsAsync(
        IEnumerable<int> dailyMealIds, CancellationToken ct = default);

    Task<ScheduleMeal?> GetByIdAsync(
        long scheduleMealId,
        CancellationToken ct = default);

    Task<IReadOnlyList<FoodIngredientInfo>> GetFoodIngredientsForFoodsAsync(
    IEnumerable<int> foodIds,
    CancellationToken ct = default);
}
