using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SMMS.Application.Features.foodmenu.DTOs;
using SMMS.Application.Features.Skeleton.Interfaces;
using SMMS.Domain.Entities.foodmenu;

namespace SMMS.Application.Features.foodmenu.Interfaces;
public interface IFeedbackRepository
{
    Task<Feedback?> GetByIdAsync(int feedbackId, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Feedback>> SearchAsync(
            Guid? schoolId,
            Guid? senderId,
            int? dailyMealId,
            string? targetType,
            string? keyword,
            DateTime? fromCreatedAt,
            DateTime? toCreatedAt,
            byte? rating,              // ✨ NEW
            string sortBy,
            bool sortDesc,
            CancellationToken cancellationToken = default);


    Task<FeedbackDto> CreateAsync(CreateFeedbackDto dto, CancellationToken ct);
    Task<IReadOnlyList<FeedbackDto>> GetBySenderAsync(Guid senderId, CancellationToken ct);
}
