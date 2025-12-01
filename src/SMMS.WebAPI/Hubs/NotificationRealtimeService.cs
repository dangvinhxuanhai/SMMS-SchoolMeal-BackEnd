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
        // Chuyển ID sang chữ thường
        var userIdStrings = userIds.Select(id => id.ToString().ToLower()).ToList();

        _logger.LogInformation($"[Realtime] Sending to Groups: {string.Join(", ", userIdStrings)}");

        // 🚀 ĐỔI TỪ Clients.Users SANG Clients.Groups
        // Vì ở Hub ta đã: Groups.AddToGroupAsync(Context.ConnectionId, userId);
        foreach (var userId in userIdStrings)
        {
            await _hub.Clients.Group(userId).SendAsync("ReceiveNotification", new
            {
                notificationId = notification.NotificationId,
                title = notification.Title,
                content = notification.Content,
                createdAt = notification.CreatedAt,
                isRead = false,
                sendType = notification.SendType,
                attachmentUrl = notification.AttachmentUrl
            });
        }
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
