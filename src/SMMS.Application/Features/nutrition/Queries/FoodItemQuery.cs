using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediatR;
using SMMS.Application.Features.nutrition.DTOs;

namespace SMMS.Application.Features.nutrition.Queries;
public record GetFoodItemsQuery(
    Guid SchoolId,
    string? Keyword,
    bool IncludeInactive = false
) : IRequest<IReadOnlyList<FoodItemDto>>;

// Get by id
public record GetFoodItemByIdQuery(int FoodId)
    : IRequest<FoodItemDto?>;
