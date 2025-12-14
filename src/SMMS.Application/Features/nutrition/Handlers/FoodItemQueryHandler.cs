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

        // 2. Load FoodItemIngredients cho toàn bộ Food
        var foodIds = foods.Select(f => f.FoodId).ToList();

        var foodIngredients = new List<FoodItemIngredient>();
        foreach (var foodId in foodIds)
        {
            var ingredients = await _foodIngRepo.
                GetByFoodIdAsync(foodId, ct); foodIngredients.AddRange(ingredients);
        }

        // Map FoodId -> IngredientIds
        var ingredientIdsByFood = foodIngredients
            .GroupBy(x => x.FoodId)
            .ToDictionary(
                g => g.Key,
                g => g.Select(x => x.IngredientId).Distinct().ToList()
            );

        // 3. Lấy tổng số học sinh của trường
        var totalStudents = await _foodRepo
            .CountStudentsBySchoolAsync(request.SchoolId, ct);

        // Không có học sinh → tất cả GREEN
        if (totalStudents == 0)
        {
            return foods.Select(f => MapDto(f, AllergyRiskStatus.Green))
                        .ToList()
                        .AsReadOnly();
        }

        // 4. Lấy thống kê dị ứng theo Ingredient
        var allIngredientIds = foodIngredients
            .Select(x => x.IngredientId)
            .Distinct()
            .ToList();

        var allergyStats = await _foodRepo.GetIngredientAllergyStatsAsync(
            allIngredientIds,
            request.SchoolId,
            ct);

        var allergyCountByIngredient = allergyStats
            .ToDictionary(x => x.IngredientId, x => x.AllergicStudentCount);

        // 5. Map FoodItem → DTO + AllergyStatus
        var result = foods
            .Select(food =>
            {
                ingredientIdsByFood.TryGetValue(food.FoodId, out var ingIds);

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

                    AllergyStatus = status,
                    TotalAllergyPercent = totalPercent // 👀 tổng %
                };
            })
            .OrderBy(x => x.TotalAllergyPercent)
            .ToList()
            .AsReadOnly();

        return result;
    }

    // ===================== PRIVATE =====================

    private static FoodItemDto MapDto(
        Domain.Entities.nutrition.FoodItem food,
        AllergyRiskStatus status)
    {
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
            AllergyStatus = status
        };
    }

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

        // ⚠️ Ngưỡng tạm – bạn sẽ chỉnh sau
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
