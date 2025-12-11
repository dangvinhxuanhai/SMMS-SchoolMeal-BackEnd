using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SMMS.Domain.Entities.billing;

namespace SMMS.Application.Features.Manager.Interfaces;
public interface IManagerNotificationRepository
{
    Task<Notification?> GetByIdAsync(long id);
    Task<List<Notification>> GetBySenderAsync(Guid senderId, int page, int pageSize);
    Task<int> CountRecipientsAsync(long notificationId);

    Task<List<Guid>> GetRecipientUserIdsAsync(Guid schoolId, List<string> roleNames);
    Task DeleteRecipientsAsync(IEnumerable<NotificationRecipient> recipients);
    Task AddNotificationAsync(Notification entity);
    Task AddRecipientsAsync(List<NotificationRecipient> entities);

    Task UpdateAsync(Notification entity);
    Task DeleteAsync(Notification entity);
    Task<List<NotificationRecipient>> GetRecipientsAsync(long notificationId);
    Task<long> CountBySenderAsync(Guid senderId);
    Task SaveChangesAsync();
}
