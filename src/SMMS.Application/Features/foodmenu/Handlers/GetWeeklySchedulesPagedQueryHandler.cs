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
public class GetWeeklySchedulesPagedQueryHandler : IRequestHandler<GetWeeklySchedulesPagedQuery, PagedResult<WeeklyScheduleDto>>
{
    private readonly IScheduleMealRepository _scheduleRepo;
    private const int MAX_FETCH_ALL = 5000; // giới hạn tối đa khi getAll = true

    public GetWeeklySchedulesPagedQueryHandler(IScheduleMealRepository scheduleRepo)
    {
        _scheduleRepo = scheduleRepo;
    }

    public async Task<PagedResult<WeeklyScheduleDto>> Handle(GetWeeklySchedulesPagedQuery request, CancellationToken ct)
    {
        if (request == null) throw new ArgumentNullException(nameof(request));

        var totalCount = await _scheduleRepo.CountBySchoolAsync(request.SchoolId, ct);

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

        // Nếu client yêu cầu getAll, kiểm tra giới hạn
        if (request.GetAll && totalCount > MAX_FETCH_ALL)
        {
            // Bạn có thể đổi thành throw custom exception; controller sẽ bắt và trả BadRequest
            throw new InvalidOperationException($"Requested to fetch all records but totalCount ({totalCount}) exceeds allowed maximum ({MAX_FETCH_ALL}). Use paging or export.");
        }

        IReadOnlyList<ScheduleMeal> schedules;
        if (request.GetAll)
        {
            schedules = await _scheduleRepo.GetAllBySchoolAsync(request.SchoolId, ct);
        }
        else
        {
            schedules = await _scheduleRepo.GetPagedBySchoolAsync(request.SchoolId, request.PageIndex, request.PageSize, ct);
        }

        var scheduleIds = schedules.Select(s => s.ScheduleMealId).ToList();

        // Lấy dailyMeals cho các schedule đã lấy
        var dailyMeals = await _scheduleRepo.GetDailyMealsForSchedulesAsync(scheduleIds, ct);
        var dailyMealIds = dailyMeals.Select(d => d.DailyMealId).ToList();

        // Lấy menu food items cho các dailyMealIds
        var menuFoods = await _scheduleRepo.GetMenuFoodItemsForDailyMealsAsync(dailyMealIds, ct);

        // Lấy list FoodId để query nguyên liệu
        var allFoodIds = menuFoods
            .Select(m => m.FoodId)
            .Distinct()
            .ToList();

        // Lấy danh sách nguyên liệu + gram cho tất cả món trong tuần
        var foodIngredients = await _scheduleRepo.GetFoodIngredientsForFoodsAsync(allFoodIds, ct);

        // Group theo FoodId để dễ map
        var ingredientsByFood = foodIngredients
            .GroupBy(fi => fi.FoodId)
            .ToDictionary(
                g => g.Key,
                g => g.ToList()
            );

        // group dữ liệu để map sang DTO
        var menuFoodsByDaily = menuFoods
            .GroupBy(m => m.DailyMealId)
            .ToDictionary(
                g => g.Key,
                g => g.OrderBy(x => x.SortOrder ?? int.MaxValue).ToList());

        var dailyBySchedule = dailyMeals
            .GroupBy(d => d.ScheduleMealId)
            .ToDictionary(
                g => g.Key,
                g => g.OrderBy(d => d.MealDate)
                      .ThenBy(d => d.MealType)
                      .ToList());

        // Map schedules -> WeeklyScheduleDto
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

        // Nếu client lấy all, trả PageIndex = 1, PageSize = totalCount để client hiểu
        var resultPageIndex = request.GetAll ? 1 : request.PageIndex;
        var resultPageSize = request.GetAll ? items.Count : request.PageSize;

        return new PagedResult<WeeklyScheduleDto>
        {
            PageIndex = resultPageIndex,
            PageSize = resultPageSize,
            TotalCount = totalCount,
            Items = items
        };
    }
}
