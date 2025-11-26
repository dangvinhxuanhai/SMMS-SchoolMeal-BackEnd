using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediatR;
using SMMS.Application.Features.nutrition.DTOs;

namespace SMMS.Application.Features.nutrition.Queries;
public record GetIngredientsQuery(
    Guid SchoolId,
    string? Keyword,
    bool IncludeInactive = false
) : IRequest<IReadOnlyList<IngredientDto>>;

public record GetIngredientByIdQuery(int IngredientId)
    : IRequest<IngredientDto?>;
