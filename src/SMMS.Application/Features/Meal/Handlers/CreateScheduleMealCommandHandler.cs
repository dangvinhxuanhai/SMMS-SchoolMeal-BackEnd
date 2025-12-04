using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediatR;
using SMMS.Application.Abstractions;
using SMMS.Application.Features.foodmenu.Interfaces;
using SMMS.Application.Features.Identity.Interfaces;
using SMMS.Application.Features.Meal.Command;
using SMMS.Application.Features.Meal.Interfaces;
using SMMS.Domain.Entities.foodmenu;

namespace SMMS.Application.Features.Meal.Handlers;
public class CreateScheduleMealCommandHandler
    : IRequestHandler<CreateScheduleMealCommand, long>
{
    private readonly IMenuRepository _menuRepository;
    private readonly IScheduleMealRepository _scheduleMealRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CreateScheduleMealCommandHandler(
        IMenuRepository menuRepository,
        IScheduleMealRepository scheduleMealRepository,
        IUnitOfWork unitOfWork)
    {
        _menuRepository = menuRepository;
        _scheduleMealRepository = scheduleMealRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<long> Handle(CreateScheduleMealCommand request, CancellationToken cancellationToken)
    {
        // 1. Chống trùng / overlap tuần
        var conflict = await _scheduleMealRepository.GetForDateAsync(
            request.SchoolId,
            request.WeekStart.Date,
            cancellationToken);

        if (conflict != null)
        {
            // Tuỳ bạn thông điệp, có thể trả chi tiết tuần đang bị đụng
            throw new InvalidOperationException(
                $"School already has a schedule from {conflict.WeekStart:yyyy-MM-dd} " +
                $"to {conflict.WeekEnd:yyyy-MM-dd}. " +
                "WeekStart must not be inside that 7-day range.");
        }

        // 2. Chuẩn bị menu template
        Menu menu;
        if (request.BaseMenuId.HasValue)
        {
            menu = await _menuRepository.GetWithDetailsAsync(request.BaseMenuId.Value, cancellationToken)
                   ?? throw new KeyNotFoundException($"Menu with id {request.BaseMenuId.Value} was not found.");
        }
        else
        {
            menu = BuildMenuTemplateFromRequest(request);
            await _menuRepository.AddAsync(menu, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }

        // 3. Tạo ScheduleMeal (không có MenuId trong schema mới)
        var scheduleMeal = new ScheduleMeal
        {
            SchoolId = request.SchoolId,
            WeekStart = DateOnly.FromDateTime(request.WeekStart),
            WeekEnd = DateOnly.FromDateTime(request.WeekEnd),
            WeekNo = request.WeekNo,
            YearNo = request.YearNo,
            Status = "Draft",
            CreatedAt = DateTime.UtcNow,
            CreatedBy = request.CreatedByUserId
        };

        scheduleMeal.DailyMeals = new List<DailyMeal>();

        // 4. Build DailyMeals + MenuFoodItems
        if (request.BaseMenuId.HasValue && (request.DailyMeals == null || request.DailyMeals.Count == 0))
        {
            CopyFromTemplateToSchedule(menu, scheduleMeal, request.WeekStart.Date);
        }
        else
        {
            BuildScheduleFromRequest(request, scheduleMeal);
        }

        await _scheduleMealRepository.AddAsync(scheduleMeal, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return scheduleMeal.ScheduleMealId;
    }

    // ================== Helper methods ======================

    /// <summary>
    /// Tạo Menu + MenuDays + MenuDayFoodItems từ danh sách DailyMeals gửi lên.
    /// 1 DailyMeal -> 1 MenuDay (DayOfWeek + MealType).
    /// </summary>
    private static Menu BuildMenuTemplateFromRequest(CreateScheduleMealCommand request)
    {
        var menu = new Menu
        {
            SchoolId = request.SchoolId,
            WeekNo = request.WeekNo,
            YearId = request.AcademicYearId,
            CreatedAt = DateTime.UtcNow,
            IsVisible = true,
            AskToDelete = false,
            MenuDays = new List<MenuDay>()
        };

        // group theo (DayOfWeek, MealType)
        var groups = request.DailyMeals
            .GroupBy(d => new
            {
                DayOfWeek = ToDbDayOfWeek(d.MealDate),
                d.MealType
            });

        foreach (var g in groups)
        {
            var menuDay = new MenuDay
            {
                DayOfWeek = g.Key.DayOfWeek,
                MealType = g.Key.MealType,
                Notes = null,
                MenuDayFoodItems = new List<MenuDayFoodItem>()
            };

            // Collect distinct food cho group đó
            var foodIds = g.SelectMany(x => x.FoodIds).Distinct().ToList();

            for (int i = 0; i < foodIds.Count; i++)
            {
                menuDay.MenuDayFoodItems.Add(new MenuDayFoodItem
                {
                    FoodId = foodIds[i],
                    SortOrder = i + 1
                });
            }

            menu.MenuDays.Add(menuDay);
        }

        return menu;
    }

    /// <summary>
    /// Copy MenuDays + MenuDayFoodItems -> DailyMeals + MenuFoodItems cho week hiện tại.
    /// </summary>
    private static void CopyFromTemplateToSchedule(Menu menu, ScheduleMeal schedule, DateTime weekStart)
    {
        foreach (var menuDay in menu.MenuDays)
        {
            var mealDate = FromDbDayOfWeek(menuDay.DayOfWeek, weekStart);

            var dailyMeal = new DailyMeal
            {
                MealDate = DateOnly.FromDateTime(mealDate),
                MealType = menuDay.MealType,
                Notes = menuDay.Notes,
                MenuFoodItems = new List<MenuFoodItem>()
            };

            foreach (var mdFood in menuDay.MenuDayFoodItems)
            {
                dailyMeal.MenuFoodItems.Add(new MenuFoodItem
                {
                    FoodId = mdFood.FoodId,
                    SortOrder = mdFood.SortOrder
                });
            }

            schedule.DailyMeals.Add(dailyMeal);
        }
    }

    /// <summary>
    /// Build DailyMeals + MenuFoodItems trực tiếp từ request (không dùng template).
    /// </summary>
    private static void BuildScheduleFromRequest(CreateScheduleMealCommand request, ScheduleMeal schedule)
    {
        foreach (var dto in request.DailyMeals)
        {
            var dailyMeal = new DailyMeal
            {
                MealDate = DateOnly.FromDateTime(dto.MealDate.Date),
                MealType = dto.MealType,
                Notes = dto.Notes,
                MenuFoodItems = new List<MenuFoodItem>()
            };

            for (int i = 0; i < dto.FoodIds.Count; i++)
            {
                dailyMeal.MenuFoodItems.Add(new MenuFoodItem
                {
                    FoodId = dto.FoodIds[i],
                    SortOrder = i + 1
                });
            }

            schedule.DailyMeals.Add(dailyMeal);
        }
    }

    /// <summary>
    /// Convert DateTime.DayOfWeek -> 1..7 (1=Mon ... 7=Sun) đúng theo cột DayOfWeek của MenuDays.
    /// </summary>
    private static byte ToDbDayOfWeek(DateTime date)
    {
        return date.DayOfWeek switch
        {
            DayOfWeek.Monday => 1,
            DayOfWeek.Tuesday => 2,
            DayOfWeek.Wednesday => 3,
            DayOfWeek.Thursday => 4,
            DayOfWeek.Friday => 5,
            DayOfWeek.Saturday => 6,
            DayOfWeek.Sunday => 7,
            _ => 1
        };
    }

    /// <summary>
    /// Map 1..7 (bảng MenuDays.DayOfWeek) -> date thực tế trong tuần theo WeekStart.
    /// Giả định WeekStart là thứ 2 (Monday).
    /// </summary>
    private static DateTime FromDbDayOfWeek(byte dayOfWeek, DateTime weekStart)
    {
        // dayOfWeek: 1=Mon -> offset 0
        var offset = (int)dayOfWeek - 1;
        return weekStart.Date.AddDays(offset);
    }
}
