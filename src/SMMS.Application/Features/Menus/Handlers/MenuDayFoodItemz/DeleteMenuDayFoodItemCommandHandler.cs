using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediatR;
using SMMS.Application.Common.Interfaces;
using SMMS.Application.Features.Menus.Command.MenuDayFoodItemz;
using SMMS.Domain.Entities.foodmenu;

namespace SMMS.Application.Features.Menus.Handlers.MenuDayFoodItemz;
public sealed class DeleteMenuDayFoodItemCommandHandler
    : IRequestHandler<DeleteMenuDayFoodItemCommand, bool>
{
    private readonly IReadRepository<MenuDayFoodItem, int> _read;
    private readonly IWriteRepository<MenuDayFoodItem, int> _write;

    public DeleteMenuDayFoodItemCommandHandler(IReadRepository<MenuDayFoodItem, int> read, IWriteRepository<MenuDayFoodItem, int> write)
    {
        _read = read;
        _write = write;
    }

    public async Task<bool> Handle(DeleteMenuDayFoodItemCommand request, CancellationToken ct)
    {
        var item = (await _read.GetAllAsync(ct))
            .FirstOrDefault(x => x.MenuDayId == request.MenuDayId && x.FoodId == request.FoodId);
        if (item is null) return false;

        // Vì repo DeleteByIdAsync dùng key đơn, ta xóa theo entity
        await _write.DeleteAsync(item, ct);
        await _write.SaveChangeAsync(ct);
        return true;
    }
}
