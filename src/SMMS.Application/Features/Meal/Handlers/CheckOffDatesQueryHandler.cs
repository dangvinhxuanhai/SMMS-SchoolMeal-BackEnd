using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediatR;
using SMMS.Application.Features.Meal.DTOs;
using SMMS.Application.Features.Meal.Queries;
using SMMS.Application.Features.notification.Interfaces;

namespace SMMS.Application.Features.Meal.Handlers;
public class CheckOffDatesQueryHandler
    : IRequestHandler<CheckOffDatesQuery, OffDateCheckResultDto>
{
    private readonly INotificationRepository _repo;

    public CheckOffDatesQueryHandler(INotificationRepository repo)
    {
        _repo = repo;
    }

    public async Task<OffDateCheckResultDto> Handle(
        CheckOffDatesQuery request,
        CancellationToken ct)
    {
        if (request.FromDate > request.ToDate)
            throw new InvalidOperationException("FromDate must be before ToDate.");

        var offDates = await _repo.GetOffDatesAsync(
            request.SchoolId,
            request.FromDate,
            request.ToDate,
            ct);

        var result = new OffDateCheckResultDto
        {
            HasOffDates = offDates.Any()
        };

        foreach (var date in offDates)
        {
            result.OffDates.Add(new OffDateItemDto
            {
                Date = date,
                DayOfWeek = date.DayOfWeek.ToString()
            });
        }

        return result;
    }
}
