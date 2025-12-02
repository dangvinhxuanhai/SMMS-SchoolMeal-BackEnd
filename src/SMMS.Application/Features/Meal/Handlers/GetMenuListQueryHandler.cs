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
public class GetMenuListQueryHandler
    : IRequestHandler<GetMenuListQuery, List<KsMenuListItemDto>>
{
    private readonly IMenuRepository _menuRepository;

    public GetMenuListQueryHandler(IMenuRepository menuRepository)
    {
        _menuRepository = menuRepository;
    }

    public async Task<List<KsMenuListItemDto>> Handle(
        GetMenuListQuery request,
        CancellationToken cancellationToken)
    {
        var menus = await _menuRepository.GetListBySchoolAsync(
            request.SchoolId,
            request.YearId,
            request.WeekNo,
            cancellationToken);

        return menus.Select(m => new KsMenuListItemDto
        {
            MenuId = m.MenuId,
            SchoolId = m.SchoolId,
            WeekNo = m.WeekNo,
            YearId = m.YearId,
            CreatedAt = m.CreatedAt,
            PublishedAt = m.PublishedAt,
            IsVisible = m.IsVisible,
            AskToDelete = m.AskToDelete
        }).ToList();
    }
}
