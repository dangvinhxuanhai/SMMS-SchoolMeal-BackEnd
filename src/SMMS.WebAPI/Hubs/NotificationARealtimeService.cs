using Microsoft.AspNetCore.SignalR;
using SMMS.Application.Features.billing.DTOs;
using SMMS.Application.Features.billing.Interfaces;
using SMMS.Application.Features.Manager.Interfaces;
using SMMS.Domain.Entities.billing;

namespace SMMS.WebAPI.Hubs;

public class NotificationARealtimeService : INotificationARealtimeService
{
    private readonly IHubContext<NotificationHub> _hub;

    public NotificationARealtimeService(IHubContext<NotificationHub> hub)
    {
        _hub = hub;
    }

    public async Task SendToUsersAsync(IEnumerable<Guid> userIds, AdminNotificationDto notification)
    {
        if (userIds == null || !userIds.Any()) return;
        var userIdStrings = userIds.Select(u => u.ToString()).ToList();
        await _hub.Clients.Users(userIdStrings)
            .SendAsync("ReceiveNotification", notification);
    }

    public async Task BroadcastToAllAsync(AdminNotificationDto notification)
    {
        await _hub.Clients.All.SendAsync("ReceiveNotification", notification);
    }

    public async Task BroadcastDeletedAsync(long notificationId)
    {
        await _hub.Clients.All.SendAsync("NotificationDeleted", new { notificationId });
    }
}
