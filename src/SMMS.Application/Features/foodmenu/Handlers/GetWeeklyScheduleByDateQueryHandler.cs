using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediatR;
using SMMS.Application.Features.foodmenu.DTOs;
using SMMS.Application.Features.foodmenu.Interfaces;
using SMMS.Application.Features.foodmenu.Queries;

namespace SMMS.Application.Features.foodmenu.Handlers;
public sealed class GetWeeklyScheduleByDateQueryHandler
    : IRequestHandler<GetWeeklyScheduleByDateQuery, WeeklyScheduleDto?>
{
    private readonly IScheduleMealRepository _scheduleRepo;

    public GetWeeklyScheduleByDateQueryHandler(IScheduleMealRepository scheduleRepo)
    {
        _scheduleRepo = scheduleRepo;
    }

    public async Task<WeeklyScheduleDto?> Handle(
        GetWeeklyScheduleByDateQuery request,
        CancellationToken ct)
    {
        // 1. Tìm ScheduleMeal theo ngày (WeekStart <= date <= WeekEnd)
        var schedule = await _scheduleRepo.GetForDateAsync(request.SchoolId, request.Date, ct);
        if (schedule == null)
            return null;

        // 2. Lấy DailyMeals của tuần này
        var dailyMeals = await _scheduleRepo.GetDailyMealsForScheduleAsync(schedule.ScheduleMealId, ct);
        var dailyMealIds = dailyMeals.Select(d => d.DailyMealId).ToList();

        // 3. Lấy món cho các DailyMeal
        var menuFoods = await _scheduleRepo.GetMenuFoodItemsForDailyMealsAsync(dailyMealIds, ct);
        var menuFoodsByDaily = menuFoods
            .GroupBy(m => m.DailyMealId)
            .ToDictionary(
                g => g.Key,
                g => g.OrderBy(x => x.SortOrder ?? int.MaxValue).ToList());

        // 4. Map ra DTO
        var dayDtos = dailyMeals
            .OrderBy(d => d.MealDate)
            .ThenBy(d => d.MealType)
            .Select(dm =>
            {
                menuFoodsByDaily.TryGetValue(dm.DailyMealId, out var foodsForDay);
                var foodDtos = (foodsForDay ?? new List<MenuFoodItemInfo>())
                    .Select(f => new ScheduledFoodItemDto
                    {
                        FoodId = f.FoodId,
                        FoodName = f.FoodName,
                        FoodType = f.FoodType,
                        IsMainDish = f.IsMainDish,
                        ImageUrl = f.ImageUrl,
                        FoodDesc = f.FoodDesc,
                        SortOrder = f.SortOrder
                    })
                    .ToList();

                return new DailyMealDto
                {
                    DailyMealId = dm.DailyMealId,
                    MealDate = dm.MealDate.ToDateTime(TimeOnly.MinValue),
                    MealType = dm.MealType,
                    Notes = dm.Notes,
                    FoodItems = foodDtos
                };
            })
            .ToList();

        return new WeeklyScheduleDto
        {
            ScheduleMealId = schedule.ScheduleMealId,
            WeekStart = schedule.WeekStart.ToDateTime(TimeOnly.MinValue),
            WeekEnd = schedule.WeekEnd.ToDateTime(TimeOnly.MinValue),
            WeekNo = schedule.WeekNo,
            YearNo = schedule.YearNo,
            Status = schedule.Status,
            Notes = schedule.Notes,
            DailyMeals = dayDtos
        };
    }
}
