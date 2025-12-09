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

        // Lấy nguyên liệu cho tất cả món trong tuần
        var allFoodIds = menuFoods
            .Select(m => m.FoodId)
            .Distinct()
            .ToList();

        var foodIngredients = await _scheduleRepo.GetFoodIngredientsForFoodsAsync(allFoodIds, ct);

        var ingredientsByFood = foodIngredients
            .GroupBy(fi => fi.FoodId)
            .ToDictionary(
                g => g.Key,
                g => g.ToList()
            );

        // 4. Map ra DTO – GỘP THEO NGÀY
        var groupedByDate = dailyMeals
            .GroupBy(dm => dm.MealDate)               // group theo DATE
            .OrderBy(g => g.Key);                     // sort theo ngày

        var dayDtos = groupedByDate
            .Select(g =>
            {
                var first = g.First(); // lấy 1 DailyMeal đại diện (để lấy id/ghi chú nếu cần)

                // Gộp toàn bộ món ăn của tất cả DailyMeal trong cùng ngày
                var allFoodsForDay = g
                    .SelectMany(dm =>
                    {
                        menuFoodsByDaily.TryGetValue(dm.DailyMealId, out var foodsForDaily);
                        return foodsForDaily ?? Enumerable.Empty<MenuFoodItemInfo>();
                    })
                    .OrderBy(f => f.SortOrder ?? int.MaxValue)
                    .ToList();

                var foodDtos = allFoodsForDay
                    .Select(f =>
                    {
                        ingredientsByFood.TryGetValue(f.FoodId, out var ingForFood);

                        var ingredientDtos = (ingForFood ?? new List<FoodIngredientInfo>())
                            .Select(i => new ScheduledFoodIngredientDto
                            {
                                IngredientId = i.IngredientId,
                                IngredientName = i.IngredientName,
                                QuantityGram = i.QuantityGram
                            })
                            .ToList()
                            .AsReadOnly();

                        return new ScheduledFoodItemDto
                        {
                            FoodId = f.FoodId,
                            FoodName = f.FoodName,
                            FoodType = f.FoodType,
                            IsMainDish = f.IsMainDish,
                            ImageUrl = f.ImageUrl,
                            FoodDesc = f.FoodDesc,
                            SortOrder = f.SortOrder,
                            Ingredients = ingredientDtos
                        };
                    })
                    .ToList();

                return new DailyMealDto
                {
                    // Nếu bạn muốn mỗi ngày 1 id "đại diện", có thể lấy id nhỏ nhất hoặc lớn nhất
                    DailyMealId = first.DailyMealId,
                    MealDate = first.MealDate.ToDateTime(TimeOnly.MinValue),

                    // ⚠️ MealType: giờ bị mix nhiều loại.
                    // 1) Nếu FE không cần, có thể để null/"" hoặc bỏ property khỏi DTO.
                    // 2) Hoặc combine cho vui:
                    MealType = string.Join(
                        ",",
                        g.Select(x => x.MealType).Distinct()),

                    // Notes: nếu nhiều Notes khác nhau, bạn cũng có thể join tương tự.
                    Notes = first.Notes,

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
