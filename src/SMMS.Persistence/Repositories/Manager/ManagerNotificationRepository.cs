using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SMMS.Application.Features.Manager.Interfaces;
using SMMS.Domain.Entities.billing;
using SMMS.Persistence.Data;

namespace SMMS.Persistence.Repositories.Manager;
public class ManagerNotificationRepository : IManagerNotificationRepository
{
    private readonly EduMealContext _context;

    public ManagerNotificationRepository(EduMealContext context)
    {
        _context = context;
    }

    public async Task<Notification?> GetByIdAsync(long id)
    {
        return await _context.Notifications
            .FirstOrDefaultAsync(n => n.NotificationId == id);
    }

    public async Task<List<Notification>> GetBySenderAsync(Guid senderId, int page, int pageSize)
    {
        return await _context.Notifications
            .Where(n => n.SenderId == senderId)
            .OrderByDescending(n => n.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
    }

    public async Task<long> CountBySenderAsync(Guid senderId)
    {
        return await _context.Notifications
            .Where(n => n.SenderId == senderId)
            .CountAsync();
    }

    public async Task<int> CountRecipientsAsync(long notificationId)
    {
        return await _context.NotificationRecipients
            .CountAsync(r => r.NotificationId == notificationId);
    }

    public async Task<List<Guid>> GetRecipientUserIdsAsync(Guid schoolId, List<string> roleNames)
    {
        var roleIds = await _context.Roles
            .Where(r => roleNames.Contains(r.RoleName) && r.IsActive)
            .Select(r => r.RoleId)
            .ToListAsync();

        return await _context.Users
            .Where(u => u.SchoolId == schoolId &&
                        u.IsActive &&
                        roleIds.Contains(u.RoleId))
            .Select(u => u.UserId)
            .ToListAsync();
    }
    public Task DeleteRecipientsAsync(IEnumerable<NotificationRecipient> recipients)
    {
        _context.NotificationRecipients.RemoveRange(recipients);
        return Task.CompletedTask;
    }
    public async Task AddNotificationAsync(Notification entity)
    {
        await _context.Notifications.AddAsync(entity);
    }

    public async Task AddRecipientsAsync(List<NotificationRecipient> entities)
    {
        await _context.NotificationRecipients.AddRangeAsync(entities);
    }

    public async Task UpdateAsync(Notification entity)
    {
        _context.Notifications.Update(entity);
    }

    public async Task DeleteAsync(Notification entity)
    {
        _context.Notifications.Remove(entity);
    }

    public async Task<List<NotificationRecipient>> GetRecipientsAsync(long notificationId)
    {
        return await _context.NotificationRecipients
            .Where(r => r.NotificationId == notificationId)
            .ToListAsync();
    }

    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }
}
