using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediatR;
using SMMS.Application.Common.Interfaces;
using SMMS.Application.Features.Menus.DTOs;
using SMMS.Application.Features.Menus.Queries;
using SMMS.Domain.Entities.nutrition;

namespace SMMS.Application.Features.Menus.Handlers;
public class GetAllFoodItemsQueryHandler : IRequestHandler<GetAllFoodItemsQuery, IReadOnlyList<FoodItemDto>>
{
    private readonly IReadRepository<FoodItem, int> _repo;

    public GetAllFoodItemsQueryHandler(IReadRepository<FoodItem, int> repo)
    {
        _repo = repo;
    }

    public async Task<IReadOnlyList<FoodItemDto>> Handle(GetAllFoodItemsQuery request, CancellationToken ct)
    {
        var items = await _repo.GetAllAsync(ct, f => f.School, f => f.CreatedByNavigation);

        var result = items.Select(f => new FoodItemDto
        {
            FoodId = f.FoodId,
            FoodName = f.FoodName,
            FoodType = f.FoodType,
            FoodDesc = f.FoodDesc,
            ImageUrl = f.ImageUrl,
            CreatedAt = f.CreatedAt,
            SchoolId = f.SchoolId,
            CreatedBy = f.CreatedBy,
            IsActive = f.IsActive
        }).ToList();

        return result;
    }
}


