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
public class CreateIngredientCommandHandler
        : IRequestHandler<CreateIngredientCommand, IngredientDto>
{
    private readonly IIngredientRepository _repository;
    private readonly IUnitOfWork _unitOfWork;

    public CreateIngredientCommandHandler(
        IIngredientRepository repository,
        IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    public async Task<IngredientDto> Handle(
        CreateIngredientCommand request,
        CancellationToken cancellationToken)
    {
        var entity = new Ingredient
        {
            IngredientName = request.IngredientName.Trim(),
            IngredientType = request.IngredientType,
            EnergyKcal = request.EnergyKcal,
            ProteinG = request.ProteinG,
            FatG = request.FatG,
            CarbG = request.CarbG,
            SchoolId = request.SchoolId,
            IsActive = true,
            CreatedBy = request.CreatedBy,
            CreatedAt = DateTime.UtcNow
        };

        await _repository.AddAsync(entity, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new IngredientDto
        {
            IngredientId = entity.IngredientId,
            IngredientName = entity.IngredientName,
            IngredientType = entity.IngredientType,
            EnergyKcal = entity.EnergyKcal,
            ProteinG = entity.ProteinG,
            FatG = entity.FatG,
            CarbG = entity.CarbG,
            SchoolId = entity.SchoolId,
            IsActive = entity.IsActive
        };
    }
}

public class UpdateIngredientCommandHandler
        : IRequestHandler<UpdateIngredientCommand, IngredientDto>
{
    private readonly IIngredientRepository _repository;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateIngredientCommandHandler(
        IIngredientRepository repository,
        IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    public async Task<IngredientDto> Handle(
        UpdateIngredientCommand request,
        CancellationToken cancellationToken)
    {
        var entity = await _repository.GetByIdAsync(request.IngredientId, cancellationToken);

        if (entity == null || !entity.IsActive)
            throw new KeyNotFoundException($"Ingredient {request.IngredientId} not found");

        entity.IngredientName = request.IngredientName.Trim();
        entity.IngredientType = request.IngredientType;
        entity.EnergyKcal = request.EnergyKcal;
        entity.ProteinG = request.ProteinG;
        entity.FatG = request.FatG;
        entity.CarbG = request.CarbG;

        await _repository.UpdateAsync(entity, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new IngredientDto
        {
            IngredientId = entity.IngredientId,
            IngredientName = entity.IngredientName,
            IngredientType = entity.IngredientType,
            EnergyKcal = entity.EnergyKcal,
            ProteinG = entity.ProteinG,
            FatG = entity.FatG,
            CarbG = entity.CarbG,
            SchoolId = entity.SchoolId,
            IsActive = entity.IsActive
        };
    }
}

public class DeleteIngredientCommandHandler
        : IRequestHandler<DeleteIngredientCommand>
{
    private readonly IIngredientRepository _repository;
    private readonly IUnitOfWork _unitOfWork;

    public DeleteIngredientCommandHandler(
        IIngredientRepository repository,
        IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(
        DeleteIngredientCommand request,
        CancellationToken cancellationToken)
    {
        var entity = await _repository.GetByIdAsync(request.IngredientId, cancellationToken);

        if (entity == null)
            throw new KeyNotFoundException($"Ingredient {request.IngredientId} not found");

        if (!request.HardDelete)
        {
            // Soft delete
            await _repository.SoftDeleteAsync(entity, cancellationToken);
        }
        else
        {
            // Hard delete: xóa tất cả quan hệ trước rồi xóa Ingredient
            await _repository.HardDeleteWithRelationsAsync(entity, cancellationToken);
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}

