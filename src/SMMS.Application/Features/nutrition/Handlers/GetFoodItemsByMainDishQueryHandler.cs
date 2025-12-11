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
public class GetFoodItemsByMainDishQueryHandler
        : IRequestHandler<GetFoodItemsByMainDishQuery, IReadOnlyList<FoodItemDto>>
{
    private readonly IFoodItemRepository _repository;

    public GetFoodItemsByMainDishQueryHandler(IFoodItemRepository repository)
    {
        _repository = repository;
    }

    public async Task<IReadOnlyList<FoodItemDto>> Handle(
        GetFoodItemsByMainDishQuery request,
        CancellationToken cancellationToken)
    {
        // dùng lại hàm GetListAsync đã có (lấy all theo trường, keyword, active)
        var entities = await _repository.GetListAsync(
            request.SchoolId,
            request.Keyword,
            request.IncludeInactive,
            cancellationToken);

        // filter theo IsMainDish nếu có
        if (request.IsMainDish.HasValue)
        {
            entities = entities
                .Where(e => e.IsMainDish == request.IsMainDish.Value)
                .ToList();
        }

        // map Entity -> DTO
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
