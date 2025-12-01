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
public sealed class CreateMenuDayCommandHandler : IRequestHandler<CreateMenuDayCommand, int>
{
    private readonly IReadRepository<MenuDay, int> _read;
    private readonly IWriteRepository<MenuDay, int> _write;

    public CreateMenuDayCommandHandler(IReadRepository<MenuDay, int> read, IWriteRepository<MenuDay, int> write)
    {
        _read = read;
        _write = write;
    }

    public async Task<int> Handle(CreateMenuDayCommand request, CancellationToken ct)
    {
        var d = request.Dto;

        // Enforce UQ (MenuId, DayOfWeek, MealType)
        var existing = (await _read.GetAllAsync(ct))
            .Any(x => x.MenuId == d.MenuId && x.DayOfWeek == d.DayOfWeek && x.MealType == d.MealType);
        if (existing)
            throw new InvalidOperationException("MenuDay already exists for this (MenuId, DayOfWeek, MealType).");

        var e = new MenuDay
        {
            MenuId = d.MenuId,
            DayOfWeek = d.DayOfWeek,
            MealType = d.MealType,
            Notes = d.Notes
        };

        await _write.AddAsync(e, ct);
        await _write.SaveChangeAsync(ct);
        return e.MenuDayId;
    }
}
