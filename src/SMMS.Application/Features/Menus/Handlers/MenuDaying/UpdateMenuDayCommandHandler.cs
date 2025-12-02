using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediatR;
using SMMS.Application.Common.Interfaces;
using SMMS.Application.Features.Menus.Command.MenuDaying;
using SMMS.Domain.Entities.foodmenu;

namespace SMMS.Application.Features.Menus.Handlers.MenuDaying;
public sealed class UpdateMenuDayCommandHandler : IRequestHandler<UpdateMenuDayCommand, bool>
{
    private readonly IReadRepository<MenuDay, int> _read;
    private readonly IWriteRepository<MenuDay, int> _write;

    public UpdateMenuDayCommandHandler(IReadRepository<MenuDay, int> read, IWriteRepository<MenuDay, int> write)
    {
        _read = read;
        _write = write;
    }

    public async Task<bool> Handle(UpdateMenuDayCommand request, CancellationToken ct)
    {
        var e = await _read.GetByIdAsync(
            request.Id,
            keyName: nameof(MenuDay.MenuDayId),
            ct
        );
        if (e is null) return false;

        var d = request.Dto;

        // Check UQ (MenuId, DayOfWeek, MealType) — avoid colliding with another row
        var conflict = (await _read.GetAllAsync(ct))
            .Any(x => x.MenuDayId != e.MenuDayId &&
                      x.MenuId == e.MenuId &&
                      x.DayOfWeek == d.DayOfWeek &&
                      x.MealType == d.MealType);
        if (conflict)
            throw new InvalidOperationException("Another MenuDay with the same (MenuId, DayOfWeek, MealType) already exists.");

        e.DayOfWeek = d.DayOfWeek;
        e.MealType = d.MealType;
        e.Notes = d.Notes;

        await _write.UpdateAsync(e, ct);
        await _write.SaveChangeAsync(ct);
        return true;
    }
}
