using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SMMS.Application.Features.Manager.DTOs;

namespace SMMS.Application.Features.Manager.Interfaces;
public interface INotificationRealtimeService
{
    Task SendToUsersAsync(IEnumerable<Guid> userIds, ManagerNotificationDto notification);
    Task BroadcastDeletedAsync(long notificationId);
}
