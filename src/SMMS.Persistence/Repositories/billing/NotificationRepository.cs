using Microsoft.EntityFrameworkCore;
using SMMS.Application.Features.notification.Interfaces;
using SMMS.Domain.Entities.billing;
using SMMS.Persistence.Data;

namespace SMMS.Infrastructure.Repositories
{
    public class NotificationRepository : INotificationRepository
    {
        private readonly EduMealContext _context;

        public NotificationRepository(EduMealContext context)
        {
            _context = context;
        }

        public IQueryable<Notification> GetAllNotifications()
        {
            return _context.Notifications
                .Include(n => n.NotificationRecipients)
                .AsQueryable();
        }

        public async Task<Notification?> GetByIdAsync(long id)
        {
            return await _context.Notifications
                .Include(n => n.NotificationRecipients)
                .ThenInclude(r => r.User)
                .FirstOrDefaultAsync(n => n.NotificationId == id);
        }

        public async Task AddNotificationAsync(Notification notification)
        {
            var users = await _context.Users.ToListAsync();
            foreach (var user in users)
            {
                notification.NotificationRecipients.Add(new NotificationRecipient
                {
                    UserId = user.UserId,
                    IsRead = false
                });
            }

            await _context.Notifications.AddAsync(notification);
            await _context.SaveChangesAsync();
        }
        public async Task DeleteNotificationAsync(Notification notification)
        {
            // Load recipients
            await _context.Entry(notification)
                .Collection(n => n.NotificationRecipients)
                .LoadAsync();

            // Xóa recipients trước
            _context.NotificationRecipients.RemoveRange(notification.NotificationRecipients);

            // Sau đó mới xóa notification
            _context.Notifications.Remove(notification);

            await _context.SaveChangesAsync();
        }

    }
}
