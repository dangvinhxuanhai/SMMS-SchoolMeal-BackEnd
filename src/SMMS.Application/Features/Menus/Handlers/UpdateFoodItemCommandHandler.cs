using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediatR;
using SMMS.Application.Common.Interfaces;
using SMMS.Application.Features.Menus.Command;
using SMMS.Domain.Entities.nutrition;

namespace SMMS.Application.Features.Menus.Handlers;
public class UpdateFoodItemCommandHandler : IRequestHandler<UpdateFoodItemCommand, bool>
{
    private readonly IReadRepository<FoodItem, int> _readRepo;
    private readonly IWriteRepository<FoodItem, int> _writeRepo;

    public UpdateFoodItemCommandHandler(
        IReadRepository<FoodItem, int> readRepo,
        IWriteRepository<FoodItem, int> writeRepo)
    {
        _readRepo = readRepo;
        _writeRepo = writeRepo;
    }

    public async Task<bool> Handle(UpdateFoodItemCommand request, CancellationToken ct)
    {
        var entity = await _readRepo.GetByIdAsync(
            request.Id,
            keyName: nameof(FoodItem.FoodId),
            ct
        );

        if (entity is null)
            return false;

        entity.FoodName = request.Dto.FoodName;
        entity.FoodType = request.Dto.FoodType;
        entity.FoodDesc = request.Dto.FoodDesc;
        entity.ImageUrl = request.Dto.ImageUrl;
        entity.IsActive = request.Dto.IsActive;

        await _writeRepo.UpdateAsync(entity, ct);
        await _writeRepo.SaveChangeAsync(ct);
        return true;
    }
}

