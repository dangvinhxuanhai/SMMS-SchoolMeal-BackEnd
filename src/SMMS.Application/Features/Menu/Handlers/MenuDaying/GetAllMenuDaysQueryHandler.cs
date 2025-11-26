using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediatR;
using SMMS.Application.Common.Interfaces;
using SMMS.Application.Features.Menu.DTOs.MenuDaying;
using SMMS.Application.Features.Menu.Queries.MenuDaying;
using SMMS.Domain.Entities.foodmenu;

namespace SMMS.Application.Features.Menu.Handlers.MenuDaying;
public sealed class GetAllMenuDaysQueryHandler
    : IRequestHandler<GetAllMenuDaysQuery, IReadOnlyList<MenuDayListItemDto>>
{
    private readonly IReadRepository<MenuDay, int> _repo;

    public GetAllMenuDaysQueryHandler(IReadRepository<MenuDay, int> repo) => _repo = repo;

    public async Task<IReadOnlyList<MenuDayListItemDto>> Handle(GetAllMenuDaysQuery request, CancellationToken ct)
    {
        var list = await _repo.GetAllAsync(ct);
        return list.Select(e => new MenuDayListItemDto
        {
            MenuDayId = e.MenuDayId,
            MenuId = e.MenuId,
            DayOfWeek = e.DayOfWeek,
            MealType = e.MealType
        }).ToList();
    }
}
