using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SMMS.Application.Common.Exceptions;
using SMMS.Application.Features.foodmenu.Interfaces;
using SMMS.Domain.Entities.foodmenu;
using SMMS.Persistence.DbContextSite;
using SMMS.Persistence.Repositories.Skeleton;

namespace SMMS.Persistence.Repositories.foodmenu;
public class FeedbackRepository : Repository<Feedback>, IFeedbackRepository
{
    public FeedbackRepository(EduMealContext dbContext) : base(dbContext)
    {
    }

    public async Task<(bool Allowed, string? Reason)> CanSendForDailyMealAsync(
        Guid parentUserId, Guid studentId, int dailyMealId, DateTime now, CancellationToken ct = default)
    {
        try
        {
            // 1) Thông tin học sinh (SchoolId + ParentId)
            var stu = await _dbContext.Students.AsNoTracking()
                .Where(x => x.StudentId == studentId)
                .Select(x => new { x.SchoolId, x.ParentId })
                .FirstOrDefaultAsync(ct);

            if (stu is null)
                return (false, "Không tìm thấy học sinh.");

            if (stu.ParentId == null || stu.ParentId.Value != parentUserId)
                return (false, "Bạn không phải phụ huynh hợp lệ của học sinh này.");

            // 2) Thông tin DailyMeal + SchoolId qua ScheduleMeal
            var dm = await (from d in _dbContext.DailyMeals.AsNoTracking()
                            join s in _dbContext.ScheduleMeals.AsNoTracking()
                                on d.ScheduleMealId equals s.ScheduleMealId
                            where d.DailyMealId == dailyMealId
                            select new
                            {
                                d.MealDate,
                                s.SchoolId,
                                s.Status
                            })
                           .FirstOrDefaultAsync(ct);

            if (dm is null)
                return (false, "Không tìm thấy bữa ăn này.");

            if (dm.SchoolId != stu.SchoolId)
                return (false, "Bữa ăn không thuộc trường của học sinh.");

            // 3) Chỉ cho gửi cho ngày đã qua
            var today = DateOnly.FromDateTime(now.Date);
            if (dm.MealDate >= today)
                return (false, "Chỉ được gửi phản hồi cho bữa ăn đã qua.");

            // (tuỳ chọn) có thể chặn nếu Schedule đang Draft, nhưng
            // thường quá khứ thì Published/Archived đều được
            // if (dm.Status == "Draft") return (false, "Thực đơn chưa công bố.");

            return (true, null);
        }
        catch (OperationCanceledException) { throw; }
        catch (Exception ex)
        {
            throw new RepositoryException(nameof(FeedbackRepository),
                nameof(CanSendForDailyMealAsync), "Lỗi kiểm tra quyền gửi feedback.", ex);
        }
    }

    public async Task<int> CreateForDailyMealAsync(
        Guid parentUserId, int dailyMealId, string content, CancellationToken ct = default)
    {
        try
        {
            // Entity scaffold thường là 'Feedback' hoặc 'Feedbacks' tuỳ tool.
            var entity = new Feedback
            {
                // FeedbackId là Identity
                SenderId = parentUserId,
                TargetType = "DailyMeal",
                TargetRef = null,
                Content = content?.Trim(),
                // CreatedAt để DEFAULT trên DB, không set cũng được
                DailyMealId = dailyMealId
            };

            _dbContext.Feedbacks.Add(entity);
            await _dbContext.SaveChangesAsync(ct);
            return entity.FeedbackId;
        }
        catch (OperationCanceledException) { throw; }
        catch (Exception ex)
        {
            throw new RepositoryException(nameof(FeedbackRepository),
                nameof(CreateForDailyMealAsync), "Lỗi tạo feedback bữa ăn.", ex);
        }
    }
}
