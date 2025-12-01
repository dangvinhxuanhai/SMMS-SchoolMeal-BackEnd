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
public class GetWeeklyScheduleByDateQueryHandler
        : IRequestHandler<GetWeeklyScheduleByDateQuery, WeeklyScheduleDto?>
{
    private readonly IScheduleMealRepository _repository;

    public GetWeeklyScheduleByDateQueryHandler(IScheduleMealRepository repository)
    {
        _repository = repository;
    }

    public async Task<WeeklyScheduleDto?> Handle(
        GetWeeklyScheduleByDateQuery request,
        CancellationToken cancellationToken)
    {
        var schedule = await _repository.GetForDateAsync(
            request.SchoolId,
            request.Date,
            cancellationToken);

        if (schedule == null)
            return null;

        var dailyMeals = await _repository.GetDailyMealsForScheduleAsync(
            schedule.ScheduleMealId,
            cancellationToken);

        var dto = new WeeklyScheduleDto
        {
            ScheduleMealId = schedule.ScheduleMealId,
            SchoolId = schedule.SchoolId,
            WeekStart = schedule.WeekStart != null ? schedule.WeekStart.ToDateTime(TimeOnly.MinValue) : DateTime.MinValue,
            WeekEnd = schedule.WeekEnd != null ? schedule.WeekEnd.ToDateTime(TimeOnly.MinValue) : DateTime.MinValue,
            WeekNo = schedule.WeekNo,
            YearNo = schedule.YearNo,
            Status = schedule.Status,
            Notes = schedule.Notes,
            DailyMeals = dailyMeals
                .Select(d => new DailyMealDto
                {
                    DailyMealId = d.DailyMealId,
                    MealDate = d.MealDate != null ? d.MealDate.ToDateTime(TimeOnly.MinValue) : DateTime.MinValue,
                    MealType = d.MealType,
                    ScheduleMealId = d.ScheduleMealId,
                    Notes = d.Notes
                })
                .ToList()
        };

        return dto;
    }
}
