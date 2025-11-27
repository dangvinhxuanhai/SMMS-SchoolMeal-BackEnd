using Microsoft.EntityFrameworkCore;
using SMMS.Application.Features.billing.Interfaces;
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
            await _context.Notifications.AddAsync(notification);
            await _context.SaveChangesAsync();
        }
        public async Task<List<Guid>> GetAllRecipientsUserIdsAsync()
        {
            return await _context.Users
                .Select(u => u.UserId)
                .ToListAsync();
        }
    }
}
