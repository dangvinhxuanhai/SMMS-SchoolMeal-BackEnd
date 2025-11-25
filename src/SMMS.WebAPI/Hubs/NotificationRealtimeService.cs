using Microsoft.AspNetCore.SignalR;
using SMMS.Application.Features.Manager.DTOs;
using SMMS.Application.Features.Manager.Interfaces;

namespace SMMS.WebAPI.Hubs;

public class NotificationRealtimeService : INotificationRealtimeService
{
    private readonly IHubContext<NotificationHub> _hub;
    private readonly ILogger<NotificationRealtimeService> _logger;

    public NotificationRealtimeService(
        IHubContext<NotificationHub> hub,
        ILogger<NotificationRealtimeService> logger)
    {
        _hub = hub;
        _logger = logger;
    }

    /// <summary>
    /// Gửi notification đến danh sách userId cụ thể.
    /// </summary>
    public async Task SendToUsersAsync(IEnumerable<Guid> userIds, ManagerNotificationDto notification)
    {
        var userIdStrings = userIds.Select(id => id.ToString()).ToList();

        _logger.LogInformation(
            "[Realtime] Sending notification {NotificationId} to {Count} users",
            notification.NotificationId,
            userIdStrings.Count
        );

        await _hub.Clients.Users(userIdStrings)
            .SendAsync("ReceiveNotification", notification);
    }

    /// <summary>
    /// Broadcast cho toàn bộ client rằng 1 thông báo bị xoá.
    /// </summary>
    public async Task BroadcastDeletedAsync(long notificationId)
    {
        _logger.LogInformation(
            "[Realtime] Broadcast delete notification {NotificationId}",
            notificationId
        );

        await _hub.Clients.All.SendAsync("NotificationDeleted", new
        {
            NotificationId = notificationId
        });
    }

    /// <summary>
    /// (Optional) Gửi tới các group theo role name (Parent, Teacher,...)
    /// => Nếu muốn mở rộng về sau.
    /// </summary>
    public async Task SendToRoleGroupAsync(string roleName, ManagerNotificationDto notification)
    {
        string groupName = $"role-{roleName}".ToLower();

        _logger.LogInformation(
            "[Realtime] Sending notification {NotificationId} to group {Group}",
            notification.NotificationId,
            groupName
        );

        await _hub.Clients.Group(groupName)
            .SendAsync("ReceiveNotification", notification);
    }
}
