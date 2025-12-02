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
public sealed class GetMenuByIdQueryHandler : IRequestHandler<GetMenuByIdQuery, MenuDetailDto?>
{
    private readonly IReadRepository<SMMS.Domain.Entities.foodmenu.Menu , int> _repo;

    public GetMenuByIdQueryHandler(IReadRepository<SMMS.Domain.Entities.foodmenu.Menu , int> repo) => _repo = repo;

    public async Task<MenuDetailDto?> Handle(GetMenuByIdQuery request, CancellationToken ct)
    {
        var e = await _repo.GetByIdAsync(
            request.Id,
            keyName: nameof(SMMS.Domain.Entities.foodmenu.Menu.MenuId),
            ct
        );
        if (e is null) return null;

        return new MenuDetailDto
        {
            MenuId = e.MenuId,
            PublishedAt = e.PublishedAt,
            SchoolId = e.SchoolId,
            IsVisible = e.IsVisible,
            WeekNo = e.WeekNo,
            CreatedAt = e.CreatedAt,
            ConfirmedBy = e.ConfirmedBy,
            ConfirmedAt = e.ConfirmedAt,
            AskToDelete = e.AskToDelete,
            YearId = e.YearId
        };
    }
}
