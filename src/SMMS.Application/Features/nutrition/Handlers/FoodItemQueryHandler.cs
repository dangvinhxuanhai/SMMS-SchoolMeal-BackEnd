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
    private readonly IFoodItemRepository _foodRepo;
    private readonly IFoodItemIngredientRepository _foodIngRepo;

    public GetFoodItemByIdQueryHandler(
        IFoodItemRepository foodRepo,
        IFoodItemIngredientRepository foodIngRepo)
    {
        _foodRepo = foodRepo;
        _foodIngRepo = foodIngRepo;
    }

    public async Task<FoodItemDto?> Handle(GetFoodItemByIdQuery request, CancellationToken token)
    {
        var food = await _foodRepo.GetByIdAsync(request.FoodId, token);
        if (food == null) return null;

        var links = await _foodIngRepo.GetByFoodIdAsync(food.FoodId, token);

        return new FoodItemDto
        {
            FoodId = food.FoodId,
            FoodName = food.FoodName,
            FoodType = food.FoodType,
            FoodDesc = food.FoodDesc,
            ImageUrl = food.ImageUrl,
            SchoolId = food.SchoolId,
            IsMainDish = food.IsMainDish,
            IsActive = food.IsActive,
            Ingredients = links.Select(l => new FoodItemIngredientDto
            {
                IngredientId = l.IngredientId,
                QuantityGram = l.QuantityGram
            }).ToList()
        };
    }
}
