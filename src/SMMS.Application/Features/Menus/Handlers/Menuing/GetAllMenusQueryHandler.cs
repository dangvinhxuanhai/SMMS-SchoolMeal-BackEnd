using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediatR;
using SMMS.Application.Common.Interfaces;
using SMMS.Application.Features.Menus.DTOs.Menuing;
using SMMS.Application.Features.Menus.Queries.Menuing;

namespace SMMS.Application.Features.Menus.Handlers.Menuing;
public sealed class GetAllMenusQueryHandler : IRequestHandler<GetAllMenusQuery, IReadOnlyList<MenuListItemDto>>
{
    private readonly IReadRepository<SMMS.Domain.Entities.foodmenu.Menu, int> _repo;

    public GetAllMenusQueryHandler(IReadRepository<SMMS.Domain.Entities.foodmenu.Menu, int> repo) => _repo = repo;

    public async Task<IReadOnlyList<MenuListItemDto>> Handle(GetAllMenusQuery request, CancellationToken ct)
    {
        var list = await _repo.GetAllAsync(ct);
        return list
            .Select(e => new MenuListItemDto
            {
                MenuId = e.MenuId,
                SchoolId = e.SchoolId,
                WeekNo = e.WeekNo,
                IsVisible = e.IsVisible,
                YearId = e.YearId,
                PublishedAt = e.PublishedAt
            })
            .ToList();
    }
}
