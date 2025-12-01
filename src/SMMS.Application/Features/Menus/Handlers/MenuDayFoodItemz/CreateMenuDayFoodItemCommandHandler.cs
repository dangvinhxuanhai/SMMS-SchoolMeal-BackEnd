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
public sealed class CreateMenuDayFoodItemCommandHandler
    : IRequestHandler<CreateMenuDayFoodItemCommand, (int MenuDayId, int FoodId)>
{
    private readonly IReadRepository<MenuDayFoodItem, int> _read;
    private readonly IWriteRepository<MenuDayFoodItem, int> _write;

    public CreateMenuDayFoodItemCommandHandler(IReadRepository<MenuDayFoodItem, int> read, IWriteRepository<MenuDayFoodItem, int> write)
    {
        _read = read;
        _write = write;
    }

    public async Task<(int MenuDayId, int FoodId)> Handle(CreateMenuDayFoodItemCommand request, CancellationToken ct)
    {
        var d = request.Dto;

        var existed = (await _read.GetAllAsync(ct))
            .Any(x => x.MenuDayId == d.MenuDayId && x.FoodId == d.FoodId);
        if (existed)
            throw new InvalidOperationException("MenuDayFoodItem already exists for (MenuDayId, FoodId).");

        var e = new MenuDayFoodItem
        {
            MenuDayId = d.MenuDayId,
            FoodId = d.FoodId,
            SortOrder = d.SortOrder
        };

        await _write.AddAsync(e, ct);
        await _write.SaveChangeAsync(ct);
        return (e.MenuDayId, e.FoodId);
    }
}
