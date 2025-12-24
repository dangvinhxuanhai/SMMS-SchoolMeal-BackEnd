using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediatR;
using SMMS.Application.Features.nutrition.DTOs;
using SMMS.Application.Features.nutrition.Interfaces;
using SMMS.Application.Features.nutrition.Queries;
using SMMS.Domain.Entities.nutrition;

namespace SMMS.Application.Features.nutrition.Handlers;
public class GetFoodItemsQueryHandler
    : IRequestHandler<GetFoodItemsQuery, IReadOnlyList<FoodItemDto>>
{
    private readonly IFoodItemRepository _foodRepo;
    private readonly IFoodItemIngredientRepository _foodIngRepo;

    public GetFoodItemsQueryHandler(
        IFoodItemRepository foodRepo,
        IFoodItemIngredientRepository foodIngRepo)
    {
        _foodRepo = foodRepo;
        _foodIngRepo = foodIngRepo;
    }

    public async Task<IReadOnlyList<FoodItemDto>> Handle(
        GetFoodItemsQuery request,
        CancellationToken ct)
    {
        // 1. Load FoodItems
        var foods = await _foodRepo.GetListAsync(
            request.SchoolId,
            request.Keyword,
            request.IncludeInactive,
            ct);

        if (foods.Count == 0)
            return Array.Empty<FoodItemDto>();

        var foodIds = foods.Select(f => f.FoodId).ToList();

        // 2. Load ALL ingredients (1 query – no N+1)
        var ingredientInfos =
            await _foodIngRepo.GetIngredientsForFoodsAsync(foodIds, ct);

        // Group IngredientInfo by FoodId
        var ingredientsByFood = ingredientInfos
            .GroupBy(x => x.FoodId)
            .ToDictionary(
                g => g.Key,
                g => g.Select(x => new FoodItemIngredientDto
                {
                    IngredientId = x.IngredientId,
                    IngredientName = x.IngredientName,
                    Unit = x.Unit,
                    QuantityGram = x.QuantityGram
                }).ToList()
            );

        // Map FoodId -> IngredientIds (for allergy calculation)
        var ingredientIdsByFood = ingredientInfos
            .GroupBy(x => x.FoodId)
            .ToDictionary(
                g => g.Key,
                g => g.Select(x => x.IngredientId).Distinct().ToList()
            );

        // 3. Total students
        var totalStudents = await _foodRepo
            .CountStudentsBySchoolAsync(request.SchoolId, ct);

        // No students → GREEN all
        if (totalStudents == 0)
        {
            return foods
                .Select(f =>
                {
                    ingredientsByFood.TryGetValue(f.FoodId, out var ings);

                    return new FoodItemDto
                    {
                        FoodId = f.FoodId,
                        FoodName = f.FoodName,
                        FoodType = f.FoodType,
                        FoodDesc = f.FoodDesc,
                        ImageUrl = f.ImageUrl,
                        SchoolId = f.SchoolId,
                        IsMainDish = f.IsMainDish,
                        IsActive = f.IsActive,
                        Ingredients = ings ?? new List<FoodItemIngredientDto>(),
                        AllergyStatus = AllergyRiskStatus.Green,
                        TotalAllergyPercent = 0
                    };
                })
                .ToList()
                .AsReadOnly();
        }

        // 4. Allergy statistics
        var allIngredientIds = ingredientInfos
            .Select(x => x.IngredientId)
            .Distinct()
            .ToList();

        var allergyStats = await _foodRepo.GetIngredientAllergyStatsAsync(
            allIngredientIds,
            request.SchoolId,
            ct);

        var allergyCountByIngredient =
            allergyStats.ToDictionary(x => x.IngredientId, x => x.AllergicStudentCount);

        // 5. Map final DTO
        var result = foods
            .Select(food =>
            {
                ingredientIdsByFood.TryGetValue(food.FoodId, out var ingIds);
                ingredientsByFood.TryGetValue(food.FoodId, out var ingredientDtos);

                var (status, totalPercent) =
                    CalculateAllergyStatusWithTotalPercent(
                        ingIds ?? Enumerable.Empty<int>(),
                        allergyCountByIngredient,
                        totalStudents);

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

                    Ingredients = ingredientDtos ?? new List<FoodItemIngredientDto>(),

                    AllergyStatus = status,
                    TotalAllergyPercent = totalPercent
                };
            })
            .OrderBy(x => x.TotalAllergyPercent)
            .ToList()
            .AsReadOnly();

        return result;
    }

    // ===================== PRIVATE =====================

    private static (AllergyRiskStatus status, double totalPercent)
        CalculateAllergyStatusWithTotalPercent(
            IEnumerable<int> ingredientIds,
            IDictionary<int, int> allergyByIngredient,
            int totalStudents)
    {
        if (totalStudents <= 0)
            return (AllergyRiskStatus.Green, 0);

        double totalRate = 0;

        foreach (var ingId in ingredientIds)
        {
            allergyByIngredient.TryGetValue(ingId, out var allergicCount);
            totalRate += (double)allergicCount / totalStudents;
        }

        var percent = Math.Round(totalRate * 100, 2);

        if (percent >= 15) return (AllergyRiskStatus.Red, percent);
        if (percent >= 5) return (AllergyRiskStatus.Orange, percent);
        return (AllergyRiskStatus.Green, percent);
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

        var ingredientInfos = await _foodIngRepo.GetIngredientsForFoodsAsync(new[] { food.FoodId }, token);

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
            Ingredients = ingredientInfos
                .Select(x => new FoodItemIngredientDto
                {
                    IngredientId = x.IngredientId,
                    IngredientName = x.IngredientName,
                    Unit = x.Unit,
                    QuantityGram = x.QuantityGram
                })
                .ToList()
        };
    }
}
