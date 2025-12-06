using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediatR;
using SMMS.Application.Features.foodmenu.DTOs;

namespace SMMS.Application.Features.foodmenu.Commands;
public record SuggestMenuCommand(
    Guid SchoolId,
    Guid UserId,
    List<int> MainIngredientIds,
    List<int> SideIngredientIds,
    List<int> AvoidAllergenIds,
    double? MaxMainKcal,
    double? MaxSideKcal,
    int TopKMain,
    int TopKSide
) : IRequest<AiMenuRecommendResponse>;
