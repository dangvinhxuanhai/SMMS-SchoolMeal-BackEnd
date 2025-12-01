using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediatR;
using SMMS.Application.Common.Interfaces;
using SMMS.Application.Features.Menus.DTOs.MenuDaying;
using SMMS.Application.Features.Menus.Queries.MenuDaying;
using SMMS.Domain.Entities.foodmenu;

namespace SMMS.Application.Features.Menus.Handlers.MenuDaying;
public sealed class GetMenuDayByIdQueryHandler
    : IRequestHandler<GetMenuDayByIdQuery, MenuDayDetailDto?>
{
    private readonly IReadRepository<MenuDay, int> _repo;

    public GetMenuDayByIdQueryHandler(IReadRepository<MenuDay, int> repo) => _repo = repo;

    public async Task<MenuDayDetailDto?> Handle(GetMenuDayByIdQuery request, CancellationToken ct)
    {
        var e = await _repo.GetByIdAsync(
            request.Id,
            keyName: nameof(MenuDay.MenuDayId),
            ct
        );
        if (e is null) return null;

        return new MenuDayDetailDto
        {
            MenuDayId = e.MenuDayId,
            MenuId = e.MenuId,
            DayOfWeek = e.DayOfWeek,
            MealType = e.MealType,
            Notes = e.Notes
        };
    }
}
