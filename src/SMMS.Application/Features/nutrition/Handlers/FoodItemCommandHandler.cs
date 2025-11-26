using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediatR;
using SMMS.Application.Abstractions;
using SMMS.Application.Features.foodmenu.Interfaces;
using SMMS.Application.Features.Identity.Interfaces;
using SMMS.Application.Features.nutrition.Commands;
using SMMS.Application.Features.nutrition.DTOs;
using SMMS.Application.Features.nutrition.Interfaces;
using SMMS.Domain.Entities.nutrition;
using SMMS.Domain.Entities.school;

namespace SMMS.Application.Features.nutrition.Handlers;
public class CreateFoodItemCommandHandler
        : IRequestHandler<CreateFoodItemCommand, FoodItemDto>
{
    private readonly IFoodItemRepository _foodItemRepository;
    private readonly IFoodItemIngredientRepository _foodItemIngredientRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IAiMenuAdminClient _aiMenuAdmin;

    public CreateFoodItemCommandHandler(
        IFoodItemRepository foodItemRepository,
        IFoodItemIngredientRepository foodItemIngredientRepository,
        IUnitOfWork unitOfWork,
        IAiMenuAdminClient aiMenuAdmin)
    {
        _foodItemRepository = foodItemRepository;
        _foodItemIngredientRepository = foodItemIngredientRepository;
        _unitOfWork = unitOfWork;
        _aiMenuAdmin = aiMenuAdmin;
    }

    public async Task<FoodItemDto> Handle(
        CreateFoodItemCommand request,
        CancellationToken cancellationToken)
    {
        var food = new FoodItem
        {
            FoodName = request.FoodName.Trim(),
            FoodType = request.FoodType,
            FoodDesc = request.FoodDesc,
            ImageUrl = request.ImageUrl,
            SchoolId = request.SchoolId,
            IsMainDish = request.IsMainDish,
            IsActive = true,
            CreatedBy = request.CreatedBy,
            CreatedAt = DateTime.UtcNow
        };

        // 1) Thêm FoodItem trước
        await _foodItemRepository.AddAsync(food, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);     // cần để lấy FoodId (IDENTITY)

        // 2) Nếu có gửi kèm Ingredients → thêm vào bảng FoodItemIngredients
        if (request.Ingredients is { Count: > 0 })
        {
            var linkEntities = request.Ingredients.Select(i => new FoodItemIngredient
            {
                FoodId = food.FoodId,
                IngredientId = i.IngredientId,
                QuantityGram = i.QuantityGram
            });

            await _foodItemIngredientRepository.ReplaceForFoodAsync(
                food.FoodId,
                linkEntities,
                cancellationToken);

            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }

        // 3) Map entity → DTO trả ra (bao gồm list Ingredients)
        var dto = new FoodItemDto
        {
            FoodId = food.FoodId,
            FoodName = food.FoodName,
            FoodType = food.FoodType,
            FoodDesc = food.FoodDesc,
            ImageUrl = food.ImageUrl,
            SchoolId = food.SchoolId,
            IsMainDish = food.IsMainDish,
            IsActive = food.IsActive,
            Ingredients = (request.Ingredients ?? new()).Select(i => new FoodItemIngredientDto
            {
                IngredientId = i.IngredientId,
                QuantityGram = i.QuantityGram
            }).ToList()
        };

        // 2. Gọi AI rebuild cho trường đó
        // MVP: gọi trực tiếp (chấp nhận hơi chậm)
        await _aiMenuAdmin.RebuildIndexAndGraphAsync(request.SchoolId, rebuildIndex: true, rebuildGraph: true, cancellationToken);


        return dto;
    }
}

public class UpdateFoodItemCommandHandler
        : IRequestHandler<UpdateFoodItemCommand, FoodItemDto>
{
    private readonly IFoodItemRepository _foodItemRepository;
    private readonly IFoodItemIngredientRepository _foodItemIngredientRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IAiMenuAdminClient _aiMenuAdmin;

    public UpdateFoodItemCommandHandler(
        IFoodItemRepository foodItemRepository,
        IFoodItemIngredientRepository foodItemIngredientRepository,
        IUnitOfWork unitOfWork,
        IAiMenuAdminClient aiMenuAdmin)
    {
        _foodItemRepository = foodItemRepository;
        _foodItemIngredientRepository = foodItemIngredientRepository;
        _unitOfWork = unitOfWork;
        _aiMenuAdmin = aiMenuAdmin;
    }

    public async Task<FoodItemDto> Handle(
        UpdateFoodItemCommand request,
        CancellationToken cancellationToken)
    {
        var food = await _foodItemRepository.GetByIdAsync(request.FoodId, cancellationToken);
        if (food == null || !food.IsActive)
            throw new KeyNotFoundException($"FoodItem {request.FoodId} not found");

        // Cập nhật thông tin món ăn
        food.FoodName = request.FoodName.Trim();
        food.FoodType = request.FoodType;
        food.FoodDesc = request.FoodDesc;
        food.ImageUrl = request.ImageUrl;
        food.IsMainDish = request.IsMainDish;

        await _foodItemRepository.UpdateAsync(food, cancellationToken);

        // Nếu client gửi Ingredients (kể cả []) -> Replace
        if (request.Ingredients != null)
        {
            var linkEntities = request.Ingredients.Select(i => new FoodItemIngredient
            {
                FoodId = food.FoodId,
                IngredientId = i.IngredientId,
                QuantityGram = i.QuantityGram
            });

            await _foodItemIngredientRepository.ReplaceForFoodAsync(
                food.FoodId,
                linkEntities,
                cancellationToken);
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Lấy lại danh sách ingredients đã lưu (cho chắc)
        var ingredLinks = await _foodItemIngredientRepository
            .GetByFoodIdAsync(food.FoodId, cancellationToken);

        var dto = new FoodItemDto
        {
            FoodId = food.FoodId,
            FoodName = food.FoodName,
            FoodType = food.FoodType,
            FoodDesc = food.FoodDesc,
            ImageUrl = food.ImageUrl,
            SchoolId = food.SchoolId,
            IsMainDish = food.IsMainDish,
            IsActive = food.IsActive,
            Ingredients = ingredLinks.Select(x => new FoodItemIngredientDto
            {
                IngredientId = x.IngredientId,
                QuantityGram = x.QuantityGram
            }).ToList()
        };

        // 2. Gọi AI rebuild cho trường đó
        // MVP: gọi trực tiếp (chấp nhận hơi chậm)
        await _aiMenuAdmin.RebuildIndexAndGraphAsync(food.SchoolId, rebuildIndex: true, rebuildGraph: true, cancellationToken);

        return dto;
    }
}

public class DeleteFoodItemCommandHandler
    : IRequestHandler<DeleteFoodItemCommand>
{
    private readonly IFoodItemRepository _repository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IAiMenuAdminClient _aiMenuAdmin;

    public DeleteFoodItemCommandHandler(
        IFoodItemRepository repository,
        IUnitOfWork unitOfWork,
        IAiMenuAdminClient aiMenuAdmin)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
        _aiMenuAdmin = aiMenuAdmin;
    }

    public async Task Handle(
        DeleteFoodItemCommand request,
        CancellationToken cancellationToken)
    {
        var entity = await _repository.GetByIdAsync(request.FoodId, cancellationToken);

        if (entity == null)
            throw new KeyNotFoundException($"FoodItem {request.FoodId} not found");

        if (!request.HardDeleteIfNoRelation)
        {
            // Soft delete
            await _repository.SoftDeleteAsync(entity, cancellationToken);
        }
        else
        {
            var hasRelations = await _repository.HasRelationsAsync(request.FoodId, cancellationToken);
            if (hasRelations)
            {
                throw new InvalidOperationException(
                    "Cannot delete FoodItem because it is used in menus, fridge samples or AI recommend results.");
            }

            await _repository.HardDeleteAsync(entity, cancellationToken);
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // 2. Gọi AI rebuild cho trường đó
        // MVP: gọi trực tiếp (chấp nhận hơi chậm)
        await _aiMenuAdmin.RebuildIndexAndGraphAsync(entity.SchoolId, rebuildIndex: true, rebuildGraph: true, cancellationToken);

    }
}
