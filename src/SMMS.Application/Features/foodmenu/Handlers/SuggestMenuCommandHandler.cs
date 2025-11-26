using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediatR;
using SMMS.Application.Features.foodmenu.Commands;
using SMMS.Application.Features.foodmenu.DTOs;
using SMMS.Application.Features.foodmenu.Interfaces;

namespace SMMS.Application.Features.foodmenu.Handlers;
public class SuggestMenuCommandHandler
        : IRequestHandler<SuggestMenuCommand, AiMenuRecommendResponse>
{
    private readonly IAiMenuClient _aiMenuClient;

    public SuggestMenuCommandHandler(IAiMenuClient aiMenuClient)
    {
        _aiMenuClient = aiMenuClient;
    }

    public async Task<AiMenuRecommendResponse> Handle(
        SuggestMenuCommand request,
        CancellationToken cancellationToken)
    {
        var aiRequest = new AiMenuRecommendRequest
        {
            UserId = request.UserId,
            SchoolId = request.SchoolId,
            MainIngredientIds = request.MainIngredientIds,
            SideIngredientIds = request.SideIngredientIds,
            AvoidAllergenIds = request.AvoidAllergenIds,
            MaxMainKcal = request.MaxMainKcal,
            MaxSideKcal = request.MaxSideKcal,
            TopKMain = request.TopKMain,
            TopKSide = request.TopKSide
        };

        return await _aiMenuClient.RecommendAsync(aiRequest, cancellationToken);
    }
}
