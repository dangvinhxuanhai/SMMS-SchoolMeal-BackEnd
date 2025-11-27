using SMMS.Domain.Entities.billing;

namespace SMMS.Application.Features.billing.Interfaces
{
    public interface INotificationRepository
    {
        IQueryable<Notification> GetAllNotifications();
        Task AddNotificationAsync(Notification notification);
        Task<Notification?> GetByIdAsync(long id);
        Task<List<Guid>> GetAllRecipientsUserIdsAsync();

    }
}
