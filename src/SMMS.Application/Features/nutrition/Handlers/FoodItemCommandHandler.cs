using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediatR;
using SMMS.Application.Abstractions;
using SMMS.Application.Features.nutrition.Commands;
using SMMS.Application.Features.nutrition.DTOs;
using SMMS.Application.Features.nutrition.Interfaces;
using SMMS.Domain.Entities.nutrition;

namespace SMMS.Application.Features.nutrition.Handlers;
public class CreateFoodItemCommandHandler
    : IRequestHandler<CreateFoodItemCommand, FoodItemDto>
{
    private readonly IFoodItemRepository _repository;
    private readonly IUnitOfWork _unitOfWork;

    public CreateFoodItemCommandHandler(
        IFoodItemRepository repository,
        IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    public async Task<FoodItemDto> Handle(
        CreateFoodItemCommand request,
        CancellationToken cancellationToken)
    {
        var entity = new FoodItem
        {
            FoodName = request.FoodName.Trim(),
            FoodType = request.FoodType,
            FoodDesc = request.FoodDesc,
            ImageUrl = request.ImageUrl,
            SchoolId = request.SchoolId,
            IsMainDish = request.IsMainDish,
            IsActive = true,
            // nếu entity có CreatedBy/CreatedAt thì set ở đây
        };

        await _repository.AddAsync(entity, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // map Entity -> DTO bằng tay
        return new FoodItemDto
        {
            FoodId = entity.FoodId,
            FoodName = entity.FoodName,
            FoodType = entity.FoodType,
            FoodDesc = entity.FoodDesc,
            ImageUrl = entity.ImageUrl,
            SchoolId = entity.SchoolId,
            IsMainDish = entity.IsMainDish,
            IsActive = entity.IsActive
        };
    }
}

public class UpdateFoodItemCommandHandler
    : IRequestHandler<UpdateFoodItemCommand, FoodItemDto>
{
    private readonly IFoodItemRepository _repository;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateFoodItemCommandHandler(
        IFoodItemRepository repository,
        IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    public async Task<FoodItemDto> Handle(
        UpdateFoodItemCommand request,
        CancellationToken cancellationToken)
    {
        var entity = await _repository.GetByIdAsync(request.FoodId, cancellationToken);

        if (entity == null || !entity.IsActive)
        {
            throw new KeyNotFoundException($"FoodItem {request.FoodId} not found");
        }

        entity.FoodName = request.FoodName.Trim();
        entity.FoodType = request.FoodType;
        entity.FoodDesc = request.FoodDesc;
        entity.ImageUrl = request.ImageUrl;
        entity.IsMainDish = request.IsMainDish;

        await _repository.UpdateAsync(entity, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new FoodItemDto
        {
            FoodId = entity.FoodId,
            FoodName = entity.FoodName,
            FoodType = entity.FoodType,
            FoodDesc = entity.FoodDesc,
            ImageUrl = entity.ImageUrl,
            SchoolId = entity.SchoolId,
            IsMainDish = entity.IsMainDish,
            IsActive = entity.IsActive
        };
    }
}

public class DeleteFoodItemCommandHandler
    : IRequestHandler<DeleteFoodItemCommand>
{
    private readonly IFoodItemRepository _repository;
    private readonly IUnitOfWork _unitOfWork;

    public DeleteFoodItemCommandHandler(
        IFoodItemRepository repository,
        IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
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
    }
}
