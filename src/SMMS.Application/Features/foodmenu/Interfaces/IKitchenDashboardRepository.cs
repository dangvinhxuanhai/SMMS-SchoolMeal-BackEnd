using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SMMS.Application.Features.foodmenu.DTOs;

namespace SMMS.Application.Features.foodmenu.Interfaces;
public interface IKitchenDashboardRepository
{
    Task<TodaySummaryDto> GetTodaySummaryAsync(
        Guid schoolId,
        DateOnly date,
        CancellationToken cancellationToken);

    Task<List<AbsenceRequestShortDto>> GetAbsenceRequestsAsync(
        Guid schoolId,
        DateOnly date,
        int take,
        CancellationToken cancellationToken);

    Task<List<FeedbackShortDto>> GetRecentFeedbacksAsync(
        Guid schoolId,
        int take,
        CancellationToken cancellationToken);

    Task<List<InventoryAlertShortDto>> GetInventoryAlertsAsync(
        Guid schoolId,
        DateOnly today,
        int take,
        CancellationToken cancellationToken);
}
