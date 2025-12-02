using SMMS.Domain.Entities.billing;

namespace SMMS.Application.Features.notification.Interfaces
{
    public interface INotificationRepository
    {
        IQueryable<Notification> GetAllNotifications();
        Task AddNotificationAsync(Notification notification);
        Task<Notification?> GetByIdAsync(long id);
        Task DeleteNotificationAsync(Notification notification);
    }
}
