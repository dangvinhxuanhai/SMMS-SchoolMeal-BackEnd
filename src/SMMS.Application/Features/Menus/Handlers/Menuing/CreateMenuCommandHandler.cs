using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediatR;
using SMMS.Application.Common.Interfaces;
using SMMS.Application.Features.Menus.Command.Menuing;

namespace SMMS.Application.Features.Menus.Handlers.Menuing;
public sealed class CreateMenuCommandHandler : IRequestHandler<CreateMenuCommand, int>
{
    private readonly IWriteRepository<SMMS.Domain.Entities.foodmenu.Menu, int> _repo;

    public CreateMenuCommandHandler(IWriteRepository<SMMS.Domain.Entities.foodmenu.Menu, int> repo) => _repo = repo;

    public async Task<int> Handle(CreateMenuCommand request, CancellationToken ct)
    {
        var d = request.Dto;
        var entity = new SMMS.Domain.Entities.foodmenu.Menu
        {
            PublishedAt = d.PublishedAt,
            SchoolId = d.SchoolId,
            IsVisible = d.IsVisible,
            WeekNo = d.WeekNo,
            ConfirmedBy = d.ConfirmedBy,
            ConfirmedAt = d.ConfirmedAt,
            AskToDelete = d.AskToDelete,
            YearId = d.YearId,
            CreatedAt = DateTime.UtcNow
        };

        await _repo.AddAsync(entity, ct);
        await _repo.SaveChangeAsync(ct);
        return entity.MenuId;
    }
}
