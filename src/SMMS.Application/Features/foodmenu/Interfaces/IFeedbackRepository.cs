using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SMMS.Application.Features.Skeleton.Interfaces;
using SMMS.Domain.Entities.foodmenu;

namespace SMMS.Application.Features.foodmenu.Interfaces;
public interface IFeedbackRepository : IRepository<Feedback>
{
    /// <summary>
    /// Kiểm tra quyền & điều kiện gửi feedback cho một DailyMeal cụ thể.
    /// </summary>
    /// <param name="parentUserId">UserId của phụ huynh đang đăng nhập</param>
    /// <param name="studentId">Học sinh mà phụ huynh xem</param>
    /// <param name="dailyMealId">Id bữa ăn trong ngày</param>
    /// <param name="now">Thời điểm hiện tại (server/local tùy chính sách)</param>
    Task<(bool Allowed, string? Reason)> CanSendForDailyMealAsync(
        Guid parentUserId, Guid studentId, int dailyMealId, DateTime now,
        CancellationToken ct = default);

    /// <summary>
    /// Tạo feedback khi đã hợp lệ. Trả về FeedbackId.
    /// </summary>
    Task<int> CreateForDailyMealAsync(
        Guid parentUserId, int dailyMealId, string content,
        CancellationToken ct = default);
}
