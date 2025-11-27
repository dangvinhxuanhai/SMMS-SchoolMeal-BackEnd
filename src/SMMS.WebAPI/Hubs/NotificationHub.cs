using DocumentFormat.OpenXml.InkML;
using DocumentFormat.OpenXml.Spreadsheet;
using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;

namespace SMMS.WebAPI.Hubs;

public class NotificationHub : Hub
{
    public override async Task OnConnectedAsync()
    {
        var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value
                     ?? Context.UserIdentifier;

        if (!string.IsNullOrEmpty(userId))
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, $"user-{userId}");
            Console.WriteLine($"✅ User {userId} connected via SignalR");
        }
        else
        {
            Console.WriteLine("⚠️ SignalR connected but no UserId found in token");
        }

        await base.OnConnectedAsync();
    }
}
