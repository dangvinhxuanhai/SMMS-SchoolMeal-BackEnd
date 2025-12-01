using Microsoft.AspNetCore.SignalR;
using SMMS.Application.Features.billing.DTOs;
using SMMS.Domain.Entities.billing;
namespace SMMS.Application.Features.billing.Interfaces
{
    public interface INotificationARealtimeService
    {
        Task SendToUsersAsync(IEnumerable<Guid> userIds, AdminNotificationDto notification);
        Task BroadcastDeletedAsync(long notificationId);
        Task BroadcastToAllAsync(AdminNotificationDto notification);
    }
}
