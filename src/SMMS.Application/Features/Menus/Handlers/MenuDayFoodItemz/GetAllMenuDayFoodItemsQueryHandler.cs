using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediatR;
using SMMS.Application.Common.Interfaces;
using SMMS.Application.Features.Menus.DTOs.MenuDayFoodItemz;
using SMMS.Application.Features.Menus.Queries.MenuDayFoodItemz;
using SMMS.Domain.Entities.foodmenu;

namespace SMMS.Application.Features.Menus.Handlers.MenuDayFoodItemz;
public sealed class GetAllMenuDayFoodItemsQueryHandler
    : IRequestHandler<GetAllMenuDayFoodItemsQuery, IReadOnlyList<MenuDayFoodItemListItemDto>>
{
    private readonly IReadRepository<MenuDayFoodItem, int> _repo;

    public GetAllMenuDayFoodItemsQueryHandler(IReadRepository<MenuDayFoodItem, int> repo)
    {
        _repo = repo;
    }

    public async Task<IReadOnlyList<MenuDayFoodItemListItemDto>> Handle(GetAllMenuDayFoodItemsQuery request, CancellationToken ct)
    {
        var list = await _repo.GetAllAsync(ct);
        return list.Select(x => new MenuDayFoodItemListItemDto
        {
            MenuDayId = x.MenuDayId,
            FoodId = x.FoodId,
            SortOrder = x.SortOrder
        }).ToList();
    }
}
