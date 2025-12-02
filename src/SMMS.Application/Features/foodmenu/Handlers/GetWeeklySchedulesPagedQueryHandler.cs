using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediatR;
using SMMS.Application.Features.foodmenu.DTOs;
using SMMS.Application.Features.foodmenu.Interfaces;
using SMMS.Application.Features.foodmenu.Queries;
using SMMS.Domain.Entities.foodmenu;

namespace SMMS.Application.Features.foodmenu.Handlers;
public sealed class GetWeeklySchedulesQueryHandler
        : IRequestHandler<GetWeeklySchedulesPagedQuery, PagedResult<WeeklyScheduleDto>>
{
    private readonly IScheduleMealRepository _scheduleRepo;

    public GetWeeklySchedulesQueryHandler(IScheduleMealRepository scheduleRepo)
    {
        _scheduleRepo = scheduleRepo;
    }

    public async Task<PagedResult<WeeklyScheduleDto>> Handle(
        GetWeeklySchedulesPagedQuery request,
        CancellationToken ct)
    {
        var totalCount = await _scheduleRepo.CountBySchoolAsync(request.SchoolId, ct);

        // Nếu không có dữ liệu, trả về PagedResult rỗng nhưng vẫn set PageIndex/PageSize
        if (totalCount == 0)
        {
            return new PagedResult<WeeklyScheduleDto>
            {
                PageIndex = request.PageIndex,
                PageSize = request.PageSize,
                TotalCount = 0,
                Items = new List<WeeklyScheduleDto>()
            };
        }

        // 1. Lấy các ScheduleMeal theo tuần (đã phân trang)
        var schedules = await _scheduleRepo.GetPagedBySchoolAsync(
            request.SchoolId,
            request.PageIndex,
            request.PageSize,
            ct);

        var scheduleIds = schedules.Select(s => s.ScheduleMealId).ToList();

        // 2. Lấy toàn bộ DailyMeals thuộc các ScheduleMeal này
        var dailyMeals = await _scheduleRepo.GetDailyMealsForSchedulesAsync(scheduleIds, ct);
        var dailyMealIds = dailyMeals.Select(d => d.DailyMealId).ToList();

        // 3. Lấy toàn bộ Food (MenuFoodItems + FoodItems) cho các DailyMeal
        var menuFoods = await _scheduleRepo.GetMenuFoodItemsForDailyMealsAsync(dailyMealIds, ct);

        // 4. Group dữ liệu để map sang DTO

        // group món theo DailyMealId
        var menuFoodsByDaily = menuFoods
            .GroupBy(m => m.DailyMealId)
            .ToDictionary(
                g => g.Key,
                g => g.OrderBy(x => x.SortOrder ?? int.MaxValue).ToList());

        // group DailyMeals theo ScheduleMealId
        var dailyBySchedule = dailyMeals
            .GroupBy(d => d.ScheduleMealId)
            .ToDictionary(
                g => g.Key,
                g => g.OrderBy(d => d.MealDate)
                      .ThenBy(d => d.MealType)
                      .ToList());

        // 5. Map ra WeeklyScheduleDto đầy đủ
        var items = schedules
            .OrderByDescending(s => s.WeekStart)
            .Select(s =>
            {
                dailyBySchedule.TryGetValue(s.ScheduleMealId, out var daysForSchedule);
                var dayDtos = (daysForSchedule ?? new List<DailyMeal>())
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
                    ScheduleMealId = s.ScheduleMealId,
                    WeekStart = s.WeekStart.ToDateTime(TimeOnly.MinValue),
                    WeekEnd = s.WeekEnd.ToDateTime(TimeOnly.MinValue),
                    WeekNo = s.WeekNo,
                    YearNo = s.YearNo,
                    Status = s.Status,
                    Notes = s.Notes,
                    DailyMeals = dayDtos
                };
            })
            .ToList();

        // 6. Trả về PagedResult với Items đã map
        return new PagedResult<WeeklyScheduleDto>
        {
            PageIndex = request.PageIndex,
            PageSize = request.PageSize,
            TotalCount = totalCount,
            Items = items
        };
    }
}
