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
public class GetWeeklySchedulesPagedQueryHandler
        : IRequestHandler<GetWeeklySchedulesPagedQuery, PagedResult<WeeklyScheduleDto>>
{
    private readonly IScheduleMealRepository _repository;

    public GetWeeklySchedulesPagedQueryHandler(IScheduleMealRepository repository)
    {
        _repository = repository;
    }

    public async Task<PagedResult<WeeklyScheduleDto>> Handle(
        GetWeeklySchedulesPagedQuery request,
        CancellationToken cancellationToken)
    {
        var total = await _repository.CountBySchoolAsync(request.SchoolId, cancellationToken);

        var result = new PagedResult<WeeklyScheduleDto>
        {
            PageIndex = request.PageIndex,
            PageSize = request.PageSize,
            TotalCount = total
        };

        if (total == 0)
            return result;

        var schedules = await _repository.GetPagedBySchoolAsync(
            request.SchoolId,
            request.PageIndex,
            request.PageSize,
            cancellationToken);

        var scheduleIds = schedules.Select(s => s.ScheduleMealId).ToList();

        var allDaily = await _repository.GetDailyMealsForSchedulesAsync(
            scheduleIds,
            cancellationToken);

        var lookup = allDaily
            .GroupBy(d => d.ScheduleMealId)
            .ToDictionary(g => g.Key, g => g.ToList());

        result.Items = schedules.Select(s =>
        {
            var dto = new WeeklyScheduleDto
            {
                ScheduleMealId = s.ScheduleMealId,
                SchoolId = s.SchoolId,
                WeekStart = s.WeekStart != null ? s.WeekStart.ToDateTime(TimeOnly.MinValue) : DateTime.MinValue,
                WeekEnd = s.WeekEnd != null ? s.WeekEnd.ToDateTime(TimeOnly.MinValue) : DateTime.MinValue,
                WeekNo = s.WeekNo,
                YearNo = s.YearNo,
                Status = s.Status,
                Notes = s.Notes
            };

            if (lookup.TryGetValue(s.ScheduleMealId, out var dList))
            {
                dto.DailyMeals = dList
                    .Select(d => new DailyMealDto
                    {
                        DailyMealId = d.DailyMealId,
                        MealDate = d.MealDate != null ? d.MealDate.ToDateTime(TimeOnly.MinValue) : DateTime.MinValue,
                        MealType = d.MealType,
                        ScheduleMealId = d.ScheduleMealId,
                        Notes = d.Notes
                    })
                    .ToList();
            }

            return dto;
        }).ToList();

        return result;
    }
}
