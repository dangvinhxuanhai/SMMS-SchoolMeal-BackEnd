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
    Task<FeedbackDto> CreateAsync(CreateFeedbackDto dto, CancellationToken ct);
    Task<IReadOnlyList<FeedbackDto>> GetBySenderAsync(Guid senderId, CancellationToken ct);
}
