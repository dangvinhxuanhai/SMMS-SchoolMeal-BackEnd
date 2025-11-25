using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SMMS.Application.Features.foodmenu.Interfaces;
using SMMS.Domain.Entities.foodmenu;
using SMMS.Persistence.Data;
using SMMS.Persistence.Repositories.Skeleton;

namespace SMMS.Persistence.Repositories.foodmenu;
public class FeedbackRepository : IFeedbackRepository
{
    private readonly EduMealContext _context;

    public FeedbackRepository(EduMealContext context)
    {
        _context = context;
    }

    public Task<Feedback?> GetByIdAsync(int feedbackId, CancellationToken cancellationToken = default)
    {
        return _context.Feedbacks
            .FirstOrDefaultAsync(f => f.FeedbackId == feedbackId, cancellationToken);
    }

    public async Task<IReadOnlyList<Feedback>> SearchAsync(
        Guid? schoolId,
        Guid? senderId,
        int? dailyMealId,
        string? targetType,
        string? keyword,
        DateTime? fromCreatedAt,
        DateTime? toCreatedAt,
        string sortBy,
        bool sortDesc,
        CancellationToken cancellationToken = default)
    {
        IQueryable<Feedback> query;

        // Nếu có filter SchoolId thì join sang ScheduleMeal để lọc theo trường
        if (schoolId.HasValue)
        {
            query =
                from f in _context.Feedbacks
                join dm in _context.DailyMeals
                    on f.DailyMealId equals dm.DailyMealId
                join sm in _context.ScheduleMeals
                    on dm.ScheduleMealId equals sm.ScheduleMealId
                where sm.SchoolId == schoolId.Value
                select f;
        }
        else
        {
            query = _context.Feedbacks.AsQueryable();
        }

        if (senderId.HasValue)
        {
            query = query.Where(f => f.SenderId == senderId.Value);
        }

        if (dailyMealId.HasValue)
        {
            query = query.Where(f => f.DailyMealId == dailyMealId.Value);
        }

        if (!string.IsNullOrWhiteSpace(targetType))
        {
            query = query.Where(f => f.TargetType == targetType);
        }

        if (!string.IsNullOrWhiteSpace(keyword))
        {
            query = query.Where(f =>
                (f.Content != null && f.Content.Contains(keyword)) ||
                (f.TargetRef != null && f.TargetRef.Contains(keyword)));
        }

        if (fromCreatedAt.HasValue)
        {
            query = query.Where(f => f.CreatedAt >= fromCreatedAt.Value);
        }

        if (toCreatedAt.HasValue)
        {
            query = query.Where(f => f.CreatedAt <= toCreatedAt.Value);
        }

        // Sort
        switch (sortBy?.Trim().ToLowerInvariant())
        {
            case "sender":
                query = sortDesc
                    ? query.OrderByDescending(f => f.SenderId)
                    : query.OrderBy(f => f.SenderId);
                break;

            case "targettype":
                query = sortDesc
                    ? query.OrderByDescending(f => f.TargetType)
                    : query.OrderBy(f => f.TargetType);
                break;

            case "createdat":
            default:
                query = sortDesc
                    ? query.OrderByDescending(f => f.CreatedAt)
                    : query.OrderBy(f => f.CreatedAt);
                break;
        }

        return await query.ToListAsync(cancellationToken);
    }
}
