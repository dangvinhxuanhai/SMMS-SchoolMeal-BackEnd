using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediatR;
using SMMS.Application.Features.Meal.DTOs;
using SMMS.Application.Features.Meal.Interfaces;
using SMMS.Application.Features.Meal.Queries;

namespace SMMS.Application.Features.Meal.Handlers;
public class GetDailyMealDetailQueryHandler
    : IRequestHandler<GetDailyMealDetailQuery, DailyMealDetailPopupDto>
{
    private readonly IDailyMealRepository _dailyMealRepo;

    public GetDailyMealDetailQueryHandler(
        IDailyMealRepository dailyMealRepo)
    {
        _dailyMealRepo = dailyMealRepo;
    }

    public async Task<DailyMealDetailPopupDto> Handle(
        GetDailyMealDetailQuery request,
        CancellationToken ct)
    {
        var result = await _dailyMealRepo
            .GetDailyMealDetailAsync(request.DailyMealId, ct);

        if (result == null)
            throw new InvalidOperationException("Daily meal not found.");

        return result;
    }
}
