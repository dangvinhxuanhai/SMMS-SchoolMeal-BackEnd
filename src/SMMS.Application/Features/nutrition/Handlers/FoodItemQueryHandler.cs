using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediatR;
using SMMS.Application.Features.nutrition.DTOs;
using SMMS.Application.Features.nutrition.Interfaces;
using SMMS.Application.Features.nutrition.Queries;

namespace SMMS.Application.Features.nutrition.Handlers;
public class GetFoodItemsQueryHandler
    : IRequestHandler<GetFoodItemsQuery, IReadOnlyList<FoodItemDto>>
{
    private readonly IFoodItemRepository _repository;

    public GetFoodItemsQueryHandler(IFoodItemRepository repository)
    {
        _repository = repository;
    }

    public async Task<IReadOnlyList<FoodItemDto>> Handle(
        GetFoodItemsQuery request,
        CancellationToken cancellationToken)
    {
        var entities = await _repository.GetListAsync(
            request.SchoolId,
            request.Keyword,
            request.IncludeInactive,
            cancellationToken);

        // Tự map Entity -> DTO (không dùng AutoMapper)
        var result = entities
            .Select(e => new FoodItemDto
            {
                FoodId = e.FoodId,
                FoodName = e.FoodName,
                FoodType = e.FoodType,
                FoodDesc = e.FoodDesc,
                ImageUrl = e.ImageUrl,
                SchoolId = e.SchoolId,
                IsMainDish = e.IsMainDish,
                IsActive = e.IsActive
            })
            .ToList()
            .AsReadOnly();

        return result;
    }
}

public class GetFoodItemByIdQueryHandler
    : IRequestHandler<GetFoodItemByIdQuery, FoodItemDto?>
{
    private readonly IFoodItemRepository _repository;

    public GetFoodItemByIdQueryHandler(IFoodItemRepository repository)
    {
        _repository = repository;
    }

    public async Task<FoodItemDto?> Handle(
        GetFoodItemByIdQuery request,
        CancellationToken cancellationToken)
    {
        var e = await _repository.GetByIdAsync(request.FoodId, cancellationToken);
        if (e == null) return null;

        // map tay
        return new FoodItemDto
        {
            FoodId = e.FoodId,
            FoodName = e.FoodName,
            FoodType = e.FoodType,
            FoodDesc = e.FoodDesc,
            ImageUrl = e.ImageUrl,
            SchoolId = e.SchoolId,
            IsMainDish = e.IsMainDish,
            IsActive = e.IsActive
        };
    }
}
