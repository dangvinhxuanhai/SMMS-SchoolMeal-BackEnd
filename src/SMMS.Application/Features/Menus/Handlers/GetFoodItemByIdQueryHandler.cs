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
public class GetFoodItemByIdQueryHandler : IRequestHandler<GetFoodItemByIdQuery, FoodItemDto?>
{
    private readonly IReadRepository<FoodItem, int> _repo;

    public GetFoodItemByIdQueryHandler(IReadRepository<FoodItem, int> repo)
    {
        _repo = repo;
    }

    public async Task<FoodItemDto?> Handle(GetFoodItemByIdQuery request, CancellationToken ct)
    {
        var entity = await _repo.GetByIdAsync(
            request.Id,
            keyName: nameof(FoodItem.FoodId),
            ct,
            f => f.School,
            f => f.CreatedByNavigation
        );

        if (entity is null) return null;

        return new FoodItemDto
        {
            FoodId = entity.FoodId,
            FoodName = entity.FoodName,
            FoodType = entity.FoodType,
            FoodDesc = entity.FoodDesc,
            ImageUrl = entity.ImageUrl,
            CreatedAt = entity.CreatedAt,
            SchoolId = entity.SchoolId,
            CreatedBy = entity.CreatedBy,
            IsActive = entity.IsActive
        };
    }
}
