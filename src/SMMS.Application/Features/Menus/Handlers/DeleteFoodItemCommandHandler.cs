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
public class DeleteFoodItemCommandHandler : IRequestHandler<DeleteFoodItemCommand, bool>
{
    private readonly IWriteRepository<FoodItem, int> _repo;

    public DeleteFoodItemCommandHandler(IWriteRepository<FoodItem, int> repo)
    {
        _repo = repo;
    }

    public async Task<bool> Handle(DeleteFoodItemCommand request, CancellationToken ct)
    {
        try
        {
            await _repo.DeleteByIdAsync(request.Id, ct);
            await _repo.SaveChangeAsync(ct);
            return true;
        }
        catch
        {
            return false;
        }
    }
}

