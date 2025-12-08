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
public class SearchFeedbacksQueryHandler
        : IRequestHandler<SearchFeedbacksQuery, IReadOnlyList<FeedbackKsDto>>
{
    private readonly IFeedbackRepository _repository;

    public SearchFeedbacksQueryHandler(IFeedbackRepository repository)
    {
        _repository = repository;
    }

    public async Task<IReadOnlyList<FeedbackKsDto>> Handle(
        SearchFeedbacksQuery request,
        CancellationToken cancellationToken)
    {
        var list = await _repository.SearchAsync(
            request.SchoolId,
            request.SenderId,
            request.DailyMealId,
            request.TargetType,
            request.Keyword,
            request.FromCreatedAt,
            request.ToCreatedAt,
            request.Rating,       // ✨ NEW
            request.SortBy,
            request.SortDesc,
            cancellationToken);


        return list
            .Select(f => new FeedbackKsDto
            {
                FeedbackId  = f.FeedbackId,
                SenderId    = f.SenderId,
                TargetType  = f.TargetType,
                TargetRef   = f.TargetRef,
                Content     = f.Content,
                CreatedAt   = f.CreatedAt,
                DailyMealId = f.DailyMealId,
                Rating      = f.Rating      // ✨ NEW
            })
            .ToList()
            .AsReadOnly();

    }
}
