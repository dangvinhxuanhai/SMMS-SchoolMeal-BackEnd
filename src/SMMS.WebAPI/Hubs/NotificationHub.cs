using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace SMMS.WebAPI.Hubs;

[Authorize]
public class NotificationHub : Hub
{
    private readonly ILogger<NotificationHub> _logger;

    public NotificationHub(ILogger<NotificationHub> logger)
    {
        _logger = logger;
    }

    public override async Task OnConnectedAsync()
    {
        var userId = Context.UserIdentifier;
        var connectionId = Context.ConnectionId;

        if (!string.IsNullOrEmpty(userId))
        {
            var normalizedUserId = userId.ToLower();

            await Groups.AddToGroupAsync(connectionId, normalizedUserId);

            _logger.LogInformation($"✅ [SignalR CONNECTED] User: '{normalizedUserId}' | ConnectionId: '{connectionId}'");
        }
        else
        {
            _logger.LogWarning($"⚠️ [SignalR WARNING] Connected but UserIdentifier is NULL. ConnectionId: '{connectionId}'");
        }

        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var userId = Context.UserIdentifier;
        _logger.LogInformation($"❌ [SignalR DISCONNECTED] User: '{userId}' | Error: {exception?.Message}");

        await base.OnDisconnectedAsync(exception);
    }
}
