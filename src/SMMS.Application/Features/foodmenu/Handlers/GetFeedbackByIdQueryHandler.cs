using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediatR;
using SMMS.Application.Features.foodmenu.DTOs;
using SMMS.Application.Features.foodmenu.Interfaces;
using SMMS.Application.Features.foodmenu.Queries;

namespace SMMS.Application.Features.foodmenu.Handlers;
public class GetFeedbackByIdQueryHandler
        : IRequestHandler<GetFeedbackByIdQuery, FeedbackKsDto?>
{
    private readonly IFeedbackRepository _repository;

    public GetFeedbackByIdQueryHandler(IFeedbackRepository repository)
    {
        _repository = repository;
    }

    public async Task<FeedbackKsDto?> Handle(
        GetFeedbackByIdQuery request,
        CancellationToken cancellationToken)
    {
        var f = await _repository.GetByIdAsync(request.FeedbackId, cancellationToken);
        if (f == null) return null;

        return new FeedbackKsDto
        {
            FeedbackId = f.FeedbackId,
            SenderId = f.SenderId,
            TargetType = f.TargetType,
            TargetRef = f.TargetRef,
            Content = f.Content,
            CreatedAt = f.CreatedAt,
            DailyMealId = f.DailyMealId,
            Rating = f.Rating      // ✨ NEW
        };
    }
}
