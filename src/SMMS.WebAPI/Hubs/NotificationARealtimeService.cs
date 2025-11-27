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
        var userIdStrings = userIds.Select(u => u.ToString()).ToList();
        await _hub.Clients.Users(userIdStrings)
            .SendAsync("ReceiveNotification", new
            {
                notification.NotificationId,
                notification.Title,
                notification.Content,
                notification.AttachmentUrl,
                notification.SenderId,
                notification.CreatedAt
            });
    }

    public async Task BroadcastDeletedAsync(long notificationId)
    {
        await _hub.Clients.All.SendAsync("NotificationDeleted", new { notificationId });
    }
}
