using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SMMS.Application.Features.foodmenu.DTOs;
using SMMS.Application.Features.foodmenu.Interfaces;
using SMMS.Persistence.DbContextSite;
using SMMS.Domain.Models.foodmenu;
using Microsoft.EntityFrameworkCore;

namespace SMMS.Application.Features.foodmenu.Service;
public sealed class WeeklyMenuService : IWeeklyMenuService
{
    private readonly EduMealContext _db;

    public WeeklyMenuService(EduMealContext dbContext)
        => _db = dbContext;

    public async Task<WeekMenuDto?> GetWeekMenuAsync(Guid studentId, DateTime anyDateInWeek, CancellationToken ct = default)
    {
        // 1) Lấy thông tin học sinh & trường
        var student = await _db.Students
            .AsNoTracking()
            .Where(s => s.StudentId == studentId)
            .Select(s => new { s.StudentId, s.SchoolId })
            .FirstOrDefaultAsync(ct);

        if (student is null) return null;

        // 2) Tìm schedule tuần Published của trường bao trùm ngày yêu cầu
        var schedule = await _db.ScheduleMeals
            .AsNoTracking()
            .Where(w => w.SchoolId == student.SchoolId
                     && w.WeekStart <= DateOnly.FromDateTime(anyDateInWeek.Date)
                     && w.WeekEnd >= DateOnly.FromDateTime(anyDateInWeek.Date)
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

        // 3) Lấy các ngày ăn trong tuần đó
        var dailyMeals = await _db.DailyMeals
            .AsNoTracking()
            .Where(d => d.ScheduleMealId == schedule.ScheduleMealId)
            .Select(d => new { d.DailyMealId, d.MealDate, d.MealType, d.Notes })
            .ToListAsync(ct);

        if (dailyMeals.Count == 0)
        {
            return new WeekMenuDto(
                schedule.SchoolId,
                (short)schedule.WeekNo,
                (short)schedule.YearNo,
                schedule.WeekStart.ToDateTime(TimeOnly.MinValue),
                schedule.WeekEnd.ToDateTime(TimeOnly.MinValue),
                schedule.Status,
                schedule.Notes,
                Array.Empty<DayMenuDto>()
            );
        }

        var dailyMealIds = dailyMeals.Select(d => d.DailyMealId).ToArray();

        // 4) Các món (FoodItems) của tuần
        var dayFoods = await _db.MenuFoodItems
            .AsNoTracking()
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

        // 5) Dị ứng của học sinh
        var studentAllergenIds = await _db.StudentAllergens
            .AsNoTracking()
            .Where(sa => sa.StudentId == studentId)
            .Select(sa => sa.AllergenId)
            .ToListAsync(ct);

        // 6) Map: FoodId -> các AllergenName khớp (từ nguyên liệu của món)
        var riskyByFoodId = new Dictionary<int, List<string>>();

        if (studentAllergenIds.Count > 0 && dayFoods.Count > 0)
        {
            var foodIds = dayFoods.Select(x => x.FoodId).Distinct().ToArray();

            var riskQuery =
                from fii in _db.FoodItemIngredients.AsNoTracking()
                where foodIds.Contains(fii.FoodId)
                join ai in _db.AllergeticIngredients.AsNoTracking()
                    on fii.IngredientId equals ai.IngredientId
                where studentAllergenIds.Contains(ai.AllergenId)
                join al in _db.Allergens.AsNoTracking()
                    on ai.AllergenId equals al.AllergenId
                select new { fii.FoodId, al.AllergenName };

            var riskPairs = await riskQuery.ToListAsync(ct);

            foreach (var g in riskPairs.GroupBy(x => x.FoodId))
            {
                riskyByFoodId[g.Key] = g.Select(x => x.AllergenName).Distinct().ToList();
            }
        }

        // 7) Build DTO cho từng ngày
        var dayLookup = dailyMeals
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
                            x.FoodId,
                            x.FoodName,
                            x.FoodType,
                            x.ImageUrl,
                            x.FoodDesc,
                            matched.Item1,
                            matched.Item2
                        );
                    })
                    .ToList();

                return new DayMenuDto(
                    d.MealDate.ToDateTime(TimeOnly.MinValue), // Convert DateOnly to DateTime
                    d.MealType,
                    d.Notes,
                    foods
                );
            })
            .ToList();

        return new WeekMenuDto(
            schedule.SchoolId,
            (short)schedule.WeekNo,
            (short)schedule.YearNo,
            schedule.WeekStart.ToDateTime(TimeOnly.MinValue),
            schedule.WeekEnd.ToDateTime(TimeOnly.MinValue),
            schedule.Status,
            schedule.Notes,
            dayLookup
        );
    }

    public async Task<IReadOnlyList<WeekOptionDto>> GetAvailableWeeksAsync(
        Guid studentId, DateTime? from = null, DateTime? to = null, CancellationToken ct = default)
    {
        var schoolId = await _db.Students
            .AsNoTracking()
            .Where(s => s.StudentId == studentId)
            .Select(s => s.SchoolId)
            .FirstOrDefaultAsync(ct);

        if (schoolId == Guid.Empty) return Array.Empty<WeekOptionDto>();

        var q = _db.ScheduleMeals
            .AsNoTracking()
            .Where(w => w.SchoolId == schoolId);

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
}
