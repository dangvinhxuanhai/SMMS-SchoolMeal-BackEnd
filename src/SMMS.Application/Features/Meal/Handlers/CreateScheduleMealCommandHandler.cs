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
using SMMS.Application.Features.Meal.DTOs;
using SMMS.Application.Features.Meal.Interfaces;
using SMMS.Application.Features.notification.Interfaces;
using SMMS.Domain.Entities.foodmenu;

namespace SMMS.Application.Features.Meal.Handlers;

public class CreateScheduleMealCommandHandler
    : IRequestHandler<CreateScheduleMealCommand, long>
{
    private readonly IMenuRepository _menuRepository;
    private readonly IScheduleMealRepository _scheduleMealRepository;
    private readonly INotificationRepository _notificationRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CreateScheduleMealCommandHandler(
        IMenuRepository menuRepository,
        IScheduleMealRepository scheduleMealRepository,
        INotificationRepository notificationRepository,
        IUnitOfWork unitOfWork)
    {
        _menuRepository = menuRepository;
        _scheduleMealRepository = scheduleMealRepository;
        _notificationRepository = notificationRepository;
        _unitOfWork = unitOfWork;
    }

    private static void ValidateWeek(DateTime weekStart, DateTime weekEnd)
    {
        if (weekStart.DayOfWeek != DayOfWeek.Monday)
            throw new InvalidOperationException("WeekStart must be Monday");

        if (weekEnd.DayOfWeek != DayOfWeek.Sunday)
            throw new InvalidOperationException("WeekEnd must be Sunday");

        if ((weekEnd.Date - weekStart.Date).TotalDays != 6)
            throw new InvalidOperationException("Week must be exactly Monday to Sunday (7 days)");
    }

    public async Task<long> Handle(CreateScheduleMealCommand request, CancellationToken cancellationToken)
    {
        ValidateWeek(request.WeekStart, request.WeekEnd);

        var conflict =
            await _scheduleMealRepository.GetForDateAsync(request.SchoolId, request.WeekStart.Date, cancellationToken);
        if (conflict != null)
        {
            throw new InvalidOperationException(
                $"Trường đã có lịch từ {conflict.WeekStart:dd/MM/yyyy} đến {conflict.WeekEnd:dd/MM/yyyy}.");
        }

        Menu menu;
        if (request.BaseMenuId.HasValue)
        {
            menu = await _menuRepository.GetWithDetailsAsync(request.BaseMenuId.Value, cancellationToken)
                   ?? throw new KeyNotFoundException($"Không tìm thấy thực đơn mẫu ID {request.BaseMenuId.Value}.");
        }
        else
        {
            menu = BuildMenuTemplateFromRequest(request);
            await _menuRepository.AddAsync(menu, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }

        var scheduleMeal = new ScheduleMeal
        {
            SchoolId = request.SchoolId,
            WeekStart = DateOnly.FromDateTime(request.WeekStart),
            WeekEnd = DateOnly.FromDateTime(request.WeekEnd),
            WeekNo = request.WeekNo,
            YearNo = request.YearNo,
            Status = "Draft",
            CreatedAt = DateTime.UtcNow,
            CreatedBy = request.CreatedByUserId,
            DailyMeals = new List<DailyMeal>()
        };

        if (request.BaseMenuId.HasValue && (request.DailyMeals == null || request.DailyMeals.Count == 0))
        {
            CopyFromTemplateToSchedule(menu, scheduleMeal, request.WeekStart.Date);
        }
        else
        {
            var offDates = await _notificationRepository.GetOffDatesAsync(
                request.SchoolId,
                DateOnly.FromDateTime(request.WeekStart),
                DateOnly.FromDateTime(request.WeekEnd),
                cancellationToken);

            BuildWeekdayDailyMeals(request, scheduleMeal, offDates);
        }

        await _scheduleMealRepository.AddAsync(scheduleMeal, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return scheduleMeal.ScheduleMealId;
    }

// ================== Helper Methods đã sửa để GỘP ======================

    private static Menu BuildMenuTemplateFromRequest(CreateScheduleMealCommand request)
    {
        var menu = new Menu
        {
            SchoolId = request.SchoolId,
            WeekNo = request.WeekNo,
            YearId = request.AcademicYearId,
            CreatedAt = DateTime.UtcNow,
            IsVisible = true,
            MenuDays = new List<MenuDay>()
        };

        var groups = request.DailyMeals
            .GroupBy(d => ToDbDayOfWeek(d.MealDate));

        foreach (var g in groups)
        {
            var menuDay = new MenuDay
            {
                DayOfWeek = g.Key,
                MealType = "Lunch",
                MenuDayFoodItems = new List<MenuDayFoodItem>()
            };

            var allFoodIds = g.SelectMany(x => x.FoodIds ?? new List<int>()).Distinct().ToList();

            for (int i = 0; i < allFoodIds.Count; i++)
            {
                menuDay.MenuDayFoodItems.Add(new MenuDayFoodItem { FoodId = allFoodIds[i], SortOrder = i + 1 });
            }

            menu.MenuDays.Add(menuDay);
        }

        return menu;
    }

    private static void CopyFromTemplateToSchedule(Menu menu, ScheduleMeal schedule, DateTime weekStart)
    {
        var groupedByDay = menu.MenuDays.GroupBy(md => md.DayOfWeek);

        foreach (var group in groupedByDay)
        {
            var mealDate = FromDbDayOfWeek(group.Key, weekStart);
            var dailyMeal = new DailyMeal
            {
                MealDate = DateOnly.FromDateTime(mealDate),
                MealType = "Lunch",
                MenuFoodItems = new List<MenuFoodItem>()
            };

            // Gom tất cả món từ các record template cùng ngày
            var allFoods = group.SelectMany(md => md.MenuDayFoodItems)
                .GroupBy(f => f.FoodId)
                .Select(f => f.First())
                .ToList();

            foreach (var f in allFoods)
            {
                dailyMeal.MenuFoodItems.Add(new MenuFoodItem { FoodId = f.FoodId, SortOrder = f.SortOrder });
            }

            schedule.DailyMeals.Add(dailyMeal);
        }
    }

    private static void BuildWeekdayDailyMeals(CreateScheduleMealCommand request, ScheduleMeal schedule,
        HashSet<DateOnly> offDates)
    {
        var requestMap = request.DailyMeals
            .GroupBy(d => d.MealDate.Date)
            .ToDictionary(g => g.Key, g => g.ToList());

        var weekStart = request.WeekStart.Date;

        for (int i = 0; i < 5; i++)
        {
            var date = weekStart.AddDays(i);
            var dateOnly = DateOnly.FromDateTime(date);

            if (offDates.Contains(dateOnly))
            {
                schedule.DailyMeals.Add(new DailyMeal
                {
                    MealDate = dateOnly,
                    MealType = "Lunch",
                    Notes = "Ngày nghỉ",
                    MenuFoodItems = new List<MenuFoodItem>()
                });
                continue;
            }

            if (requestMap.TryGetValue(date, out var mealDtos))
            {
                var allFoodIds = mealDtos.SelectMany(m => m.FoodIds ?? new List<int>()).Distinct().ToList();
                var firstDto = mealDtos.First();

                var dailyMeal = new DailyMeal
                {
                    MealDate = dateOnly,
                    MealType = "Lunch",
                    Notes = firstDto.Notes,
                    MenuFoodItems = allFoodIds
                        .Select((id, index) => new MenuFoodItem { FoodId = id, SortOrder = index + 1 }).ToList()
                };
                schedule.DailyMeals.Add(dailyMeal);
            }
            else
            {
                schedule.DailyMeals.Add(new DailyMeal
                {
                    MealDate = dateOnly, MealType = "Lunch", Notes = "TBD", MenuFoodItems = new List<MenuFoodItem>()
                });
            }
        }
    }

    private static DailyMeal CreateDailyMeal(DailyMealRequestDto dto)
    {
        var dailyMeal = new DailyMeal
        {
            MealDate = DateOnly.FromDateTime(dto.MealDate),
            MealType = string.IsNullOrWhiteSpace(dto.MealType) ? "Lunch" : dto.MealType,
            Notes = dto.Notes,
            MenuFoodItems = new List<MenuFoodItem>()
        };

        if (dto.FoodIds != null)
        {
            for (int i = 0; i < dto.FoodIds.Count; i++)
            {
                dailyMeal.MenuFoodItems.Add(new MenuFoodItem { FoodId = dto.FoodIds[i], SortOrder = i + 1 });
            }
        }

        return dailyMeal;
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
