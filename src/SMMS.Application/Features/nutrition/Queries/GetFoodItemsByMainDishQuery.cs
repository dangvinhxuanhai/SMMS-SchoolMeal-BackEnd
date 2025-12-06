using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediatR;
using SMMS.Application.Features.nutrition.DTOs;

namespace SMMS.Application.Features.nutrition.Queries;
// nullable: null = lấy tất cả, true = chỉ món chính, false = chỉ món phụ
public sealed record GetFoodItemsByMainDishQuery(
    Guid SchoolId,
    bool? IsMainDish,
    string? Keyword,
    bool IncludeInactive
) : IRequest<IReadOnlyList<FoodItemDto>>;
