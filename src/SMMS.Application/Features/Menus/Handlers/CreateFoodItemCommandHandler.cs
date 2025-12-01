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
public class CreateFoodItemCommandHandler : IRequestHandler<CreateFoodItemCommand, int>
{
    private readonly IWriteRepository<FoodItem, int> _repo;

    public CreateFoodItemCommandHandler(IWriteRepository<FoodItem, int> repo)
    {
        _repo = repo;
    }

    public async Task<int> Handle(CreateFoodItemCommand request, CancellationToken ct)
    {
        var entity = new FoodItem
        {
            FoodName = request.Dto.FoodName,
            FoodType = request.Dto.FoodType,
            FoodDesc = request.Dto.FoodDesc,
            ImageUrl = request.Dto.ImageUrl,
            SchoolId = request.Dto.SchoolId,
            CreatedBy = request.Dto.CreatedBy,
            CreatedAt = DateTime.UtcNow,
            IsActive = request.Dto.IsActive
        };

        await _repo.AddAsync(entity, ct);
        await _repo.SaveChangeAsync(ct);
        return entity.FoodId;
    }
}
