using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediatR;
using SMMS.Application.Features.Meal.DTOs;
using SMMS.Application.Features.Meal.Interfaces;
using SMMS.Application.Features.Meal.Queries;

namespace SMMS.Application.Features.Meal.Handlers;
public class GetMenuDetailQueryHandler
    : IRequestHandler<GetMenuDetailQuery, KsMenuDetailDto>
{
    private readonly IMenuRepository _menuRepository;

    public GetMenuDetailQueryHandler(IMenuRepository menuRepository)
    {
        _menuRepository = menuRepository;
    }

    public async Task<KsMenuDetailDto> Handle(
        GetMenuDetailQuery request,
        CancellationToken cancellationToken)
    {
        var menu = await _menuRepository.GetDetailWithFoodAsync(
            request.MenuId,
            request.SchoolId,
            cancellationToken);

        if (menu == null)
            throw new KeyNotFoundException($"Menu with id {request.MenuId} was not found.");

        var dto = new KsMenuDetailDto
        {
            MenuId = menu.MenuId,
            SchoolId = menu.SchoolId,
            WeekNo = menu.WeekNo,
            YearId = menu.YearId,
            CreatedAt = menu.CreatedAt,
            PublishedAt = menu.PublishedAt,
            IsVisible = menu.IsVisible,
            AskToDelete = menu.AskToDelete,
            ConfirmedAt = menu.ConfirmedAt,
            ConfirmedBy = menu.ConfirmedBy,
            Days = menu.MenuDays
                .OrderBy(d => d.DayOfWeek)
                .ThenBy(d => d.MealType)
                .Select(d => new MenuDayDto
                {
                    MenuDayId = d.MenuDayId,
                    DayOfWeek = d.DayOfWeek,
                    MealType = d.MealType,
                    Notes = d.Notes,
                    FoodItems = d.MenuDayFoodItems
                        .OrderBy(fi => fi.SortOrder)
                        .Select(fi => new MenuDayFoodItemDto
                        {
                            FoodId = fi.FoodId,
                            SortOrder = fi.SortOrder,
                            FoodName = fi.Food?.FoodName ?? string.Empty,
                            FoodType = fi.Food?.FoodType,
                            FoodDesc = fi.Food?.FoodDesc,
                            ImageUrl = fi.Food?.ImageUrl,
                            IsMainDish = fi.Food?.IsMainDish ?? false
                        }).ToList()
                }).ToList()
        };

        return dto;
    }
}
