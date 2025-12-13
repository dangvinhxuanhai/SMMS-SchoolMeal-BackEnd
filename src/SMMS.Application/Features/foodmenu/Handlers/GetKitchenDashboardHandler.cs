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
public sealed class GetKitchenDashboardHandler
        : IRequestHandler<GetKitchenDashboardQuery, KitchenDashboardDto>
{
    private readonly IKitchenDashboardRepository _repository;

    public GetKitchenDashboardHandler(IKitchenDashboardRepository repository)
    {
        _repository = repository;
    }

    public async Task<KitchenDashboardDto> Handle(
        GetKitchenDashboardQuery request,
        CancellationToken cancellationToken)
    {
        var schoolId = request.SchoolId;
        var date = request.Date;

        // Gọi song song 4 block
        var summary = await _repository.GetTodaySummaryAsync(schoolId, date, cancellationToken);
        var absences = await _repository.GetAbsenceRequestsAsync(schoolId, date, take: 5, cancellationToken);
        var feedbacks = await _repository.GetRecentFeedbacksAsync(schoolId, take: 5, cancellationToken);
        var alerts = await _repository.GetInventoryAlertsAsync(schoolId, date, take: 5, cancellationToken);


        return new KitchenDashboardDto
        {
            TodaySummary = summary,
            AbsenceRequests = absences,
            RecentFeedbacks = feedbacks,
            InventoryAlerts = alerts
        };
    }
}
