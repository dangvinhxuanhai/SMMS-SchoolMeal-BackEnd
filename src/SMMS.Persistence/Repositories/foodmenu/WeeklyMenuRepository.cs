using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SMMS.Application.Common.Exceptions;
using SMMS.Application.Features.foodmenu.DTOs;
using SMMS.Application.Features.foodmenu.Interfaces;
using SMMS.Domain.Entities.foodmenu;
using SMMS.Persistence.Data;

namespace SMMS.Persistence.Repositories.foodmenu;

public sealed class WeeklyMenuRepository : IWeeklyMenuRepository
{
    private readonly EduMealContext _db;
    private readonly ILogger<WeeklyMenuRepository> _logger;

    public WeeklyMenuRepository(EduMealContext dbContext, ILogger<WeeklyMenuRepository> logger)
    {
        _db = dbContext;
        _logger = logger;
    }

    public async Task<WeekMenuDto?> GetWeekMenuAsync(
        Guid studentId, DateTime anyDateInWeek, CancellationToken ct = default)
    {
        try
        {
            // 1) Học sinh & trường
            var student = await _db.Students.AsNoTracking()
                .Where(s => s.StudentId == studentId)
                .Select(s => new { s.StudentId, s.SchoolId })
                .FirstOrDefaultAsync(ct);

            if (student is null) return null;

            // 2) Schedule tuần Published chứa ngày yêu cầu
            var targetDate = DateOnly.FromDateTime(anyDateInWeek.Date);

            var schedule = await _db.ScheduleMeals.AsNoTracking()
                .Where(w => w.SchoolId == student.SchoolId
                            && w.WeekStart <= targetDate
                            && w.WeekEnd >= targetDate
                            && w.Status == "Published")
                .Select(w => new
                {
                    w.ScheduleMealId,
                    w.SchoolId,
                    w.WeekNo,
                    w.YearNo,
                    w.WeekStart,
                    w.WeekEnd,
                    w.Status,
                    w.Notes
                })
                .FirstOrDefaultAsync(ct);

            if (schedule is null) return null;

            // 3) Ngày ăn trong tuần
            var dailyMeals = await _db.DailyMeals.AsNoTracking()
                .Where(d => d.ScheduleMealId == schedule.ScheduleMealId)
                .Select(d => new { d.DailyMealId, d.MealDate, d.MealType, d.Notes })
                .ToListAsync(ct);

            if (dailyMeals.Count == 0)
            {
                return new WeekMenuDto(
                    schedule.SchoolId,
                    schedule.WeekNo,
                    schedule.YearNo,
                    schedule.WeekStart.ToDateTime(TimeOnly.MinValue),
                    schedule.WeekEnd.ToDateTime(TimeOnly.MinValue),
                    schedule.Status,
                    schedule.Notes,
                    Array.Empty<DayMenuDto>());
            }

            var dailyMealIds = dailyMeals.Select(d => d.DailyMealId).ToArray();

            // 4) FoodItems theo ngày
            var dayFoods = await _db.MenuFoodItems.AsNoTracking()
                .Where(mf => dailyMealIds.Contains(mf.DailyMealId))
                .Join(_db.FoodItems.AsNoTracking(),
                    mf => mf.FoodId,
                    f => f.FoodId,
                    (mf, f) => new
                    {
                        mf.DailyMealId,
                        f.FoodId,
                        f.FoodName,
                        f.FoodType,
                        f.ImageUrl,
                        f.FoodDesc
                    })
                .ToListAsync(ct);

            // 5) Dị ứng của HS
            var studentAllergenIds = await _db.StudentAllergens.AsNoTracking()
                .Where(sa => sa.StudentId == studentId)
                .Select(sa => sa.AllergenId)
                .ToListAsync(ct);

            // 6) Map FoodId -> AllergenNames trùng
            var riskyByFoodId = new Dictionary<int, List<string>>();

            if (studentAllergenIds.Count > 0 && dayFoods.Count > 0)
            {
                var foodIds = dayFoods.Select(x => x.FoodId).Distinct().ToArray();

                var riskPairs = await (
                    from fii in _db.FoodItemIngredients.AsNoTracking()
                    where foodIds.Contains(fii.FoodId)
                    join ai in _db.AllergeticIngredients.AsNoTracking()
                        on fii.IngredientId equals ai.IngredientId
                    where studentAllergenIds.Contains(ai.AllergenId)
                    join al in _db.Allergens.AsNoTracking()
                        on ai.AllergenId equals al.AllergenId
                    select new { fii.FoodId, al.AllergenName }
                ).ToListAsync(ct);

                foreach (var g in riskPairs.GroupBy(x => x.FoodId))
                    riskyByFoodId[g.Key] = g.Select(x => x.AllergenName).Distinct().ToList();
            }

            // 7) Build DTO
            var days = dailyMeals
                .OrderBy(d => d.MealDate)
                .ThenBy(d => d.MealType)
                .Select(d =>
                {
                    var foods = dayFoods
                        .Where(x => x.DailyMealId == d.DailyMealId)
                        .Select(x =>
                        {
                            var matched = riskyByFoodId.TryGetValue(x.FoodId, out var names)
                                ? (true, (IReadOnlyList<string>)names)
                                : (false, Array.Empty<string>());

                            return new MenuFoodItemDto(
                                x.FoodId, x.FoodName, x.FoodType, x.ImageUrl, x.FoodDesc,
                                matched.Item1, matched.Item2);
                        })
                        .ToList();

                    return new DayMenuDto(
                        d.MealDate.ToDateTime(TimeOnly.MinValue),
                        d.MealType,
                        d.Notes,
                        foods);
                })
                .ToList();

            return new WeekMenuDto(
                schedule.SchoolId,
                schedule.WeekNo,
                schedule.YearNo,
                schedule.WeekStart.ToDateTime(TimeOnly.MinValue),
                schedule.WeekEnd.ToDateTime(TimeOnly.MinValue),
                schedule.Status,
                schedule.Notes,
                days);
        }
        catch (OperationCanceledException)
        {
            _logger.LogWarning("GetWeekMenuAsync was canceled. studentId={StudentId}, date={Date}",
                studentId, anyDateInWeek);
            throw; // giữ nguyên semantics hủy
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "GetWeekMenuAsync failed. studentId={StudentId}, date={Date}",
                studentId, anyDateInWeek);
            throw new RepositoryException(nameof(WeeklyMenuRepository), nameof(GetWeekMenuAsync),
                "Có lỗi khi truy vấn thực đơn tuần.", ex);
        }
    }

    public async Task<IReadOnlyList<WeekOptionDto>> GetAvailableWeeksAsync(
        Guid studentId, DateTime? from = null, DateTime? to = null, CancellationToken ct = default)
    {
        try
        {
            var schoolId = await _db.Students.AsNoTracking()
                .Where(s => s.StudentId == studentId)
                .Select(s => s.SchoolId)
                .FirstOrDefaultAsync(ct);

            if (schoolId == Guid.Empty) return Array.Empty<WeekOptionDto>();

            var q = _db.ScheduleMeals.AsNoTracking()
                .Where(w => w.SchoolId == schoolId && w.Status == "Published");

            if (from.HasValue) q = q.Where(w => w.WeekEnd >= DateOnly.FromDateTime(from.Value.Date));
            if (to.HasValue) q = q.Where(w => w.WeekStart <= DateOnly.FromDateTime(to.Value.Date));

            var data = await q
                .OrderByDescending(w => w.YearNo)
                .ThenByDescending(w => w.WeekNo)
                .Select(w => new WeekOptionDto(
                    w.ScheduleMealId,
                    w.WeekNo,
                    w.YearNo,
                    w.WeekStart.ToDateTime(TimeOnly.MinValue),
                    w.WeekEnd.ToDateTime(TimeOnly.MinValue),
                    w.Status))
                .ToListAsync(ct);

            return data;
        }
        catch (OperationCanceledException)
        {
            _logger.LogWarning("GetAvailableWeeksAsync was canceled. studentId={StudentId}", studentId);
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "GetAvailableWeeksAsync failed. studentId={StudentId}", studentId);
            throw new RepositoryException(nameof(WeeklyMenuRepository), nameof(GetAvailableWeeksAsync),
                "Có lỗi khi truy vấn danh sách tuần.", ex);
        }
    }
}
