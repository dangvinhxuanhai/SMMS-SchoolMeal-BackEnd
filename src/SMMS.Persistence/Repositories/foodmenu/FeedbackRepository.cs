using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SMMS.Application.Features.foodmenu.DTOs;
using SMMS.Application.Features.foodmenu.Interfaces;
using SMMS.Domain.Entities.foodmenu;
using SMMS.Persistence.Data;

namespace SMMS.Persistence.Repositories.foodmenu
{
    public class FeedbackRepository : IFeedbackRepository
    {
        private readonly EduMealContext _db;

        public FeedbackRepository(EduMealContext db)
        {
            _db = db;
        }

        public async Task<FeedbackDto> CreateAsync(CreateFeedbackDto dto, CancellationToken ct)
        {
            // 🔥 Tạo TargetRef đúng chuẩn: "DailyMeal-{id}"
            var targetRef = $"DailyMeal-{dto.DailyMealId}";

            var entity = new Feedback
            {
                FeedbackId = 0,
                SenderId = dto.SenderId,               // id user đang đăng nhập
                TargetType = "Meal",                    // luôn là Meal
                TargetRef = targetRef,                 // Tự build
                Content = dto.Content,
                DailyMealId = dto.DailyMealId,
                CreatedAt = DateTime.UtcNow
            };

            _db.Feedbacks.Add(entity);
            await _db.SaveChangesAsync(ct);

            return new FeedbackDto(
                entity.FeedbackId,
                entity.SenderId,
                entity.TargetType,
                entity.TargetRef,
                entity.Content,
                entity.CreatedAt,
                entity.DailyMealId
            );
        }

        public async Task<IReadOnlyList<FeedbackDto>> GetBySenderAsync(Guid senderId, CancellationToken ct)
        {
            return await _db.Feedbacks
                .AsNoTracking()
                .Where(f => f.SenderId == senderId)
                .OrderByDescending(f => f.CreatedAt)
                .Select(f => new FeedbackDto(
                    f.FeedbackId,
                    f.SenderId,
                    f.TargetType,
                    f.TargetRef,
                    f.Content,
                    f.CreatedAt,
                    f.DailyMealId
                ))
                .ToListAsync(ct);
        }
    }
}
