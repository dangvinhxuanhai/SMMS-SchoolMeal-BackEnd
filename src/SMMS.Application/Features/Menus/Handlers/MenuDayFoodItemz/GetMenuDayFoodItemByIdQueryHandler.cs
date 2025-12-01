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
using SMMS.Domain.Entities.nutrition;

namespace SMMS.Application.Features.Menus.Handlers.MenuDayFoodItemz;
public sealed class GetMenuDayFoodItemByIdQueryHandler
    : IRequestHandler<GetMenuDayFoodItemByIdQuery, MenuDayFoodItemDetailDto?>
{
    private readonly IReadRepository<MenuDayFoodItem, int> _repo;
    private readonly IReadRepository<FoodItem, int> _foodRepo;

    public GetMenuDayFoodItemByIdQueryHandler(
        IReadRepository<MenuDayFoodItem, int> repo,
        IReadRepository<FoodItem, int> foodRepo)
    {
        _repo = repo;
        _foodRepo = foodRepo;
    }

    public async Task<MenuDayFoodItemDetailDto?> Handle(GetMenuDayFoodItemByIdQuery request, CancellationToken ct)
    {
        // Tìm bản ghi PK kép
        var item = (await _repo.GetAllAsync(ct))
            .FirstOrDefault(x => x.MenuDayId == request.MenuDayId && x.FoodId == request.FoodId);
        if (item is null) return null;

        // Lấy tên món
        var food = await _foodRepo.GetByIdAsync(
            item.FoodId,
            keyName: nameof(FoodItem.FoodId),
            ct
        );

        return new MenuDayFoodItemDetailDto
        {
            MenuDayId = item.MenuDayId,
            FoodId = item.FoodId,
            FoodName = food?.FoodName,
            SortOrder = item.SortOrder
        };
    }
}
