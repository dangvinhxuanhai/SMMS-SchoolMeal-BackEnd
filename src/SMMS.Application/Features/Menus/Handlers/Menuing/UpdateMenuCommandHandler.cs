using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediatR;
using SMMS.Application.Common.Interfaces;
using SMMS.Application.Features.Menus.Command.Menuing;

namespace SMMS.Application.Features.Menus.Handlers.Menuing;
public sealed class UpdateMenuCommandHandler : IRequestHandler<UpdateMenuCommand, bool>
{
    private readonly IReadRepository<SMMS.Domain.Entities.foodmenu.Menu , int> _read;
    private readonly IWriteRepository<SMMS.Domain.Entities.foodmenu.Menu , int> _write;

    public UpdateMenuCommandHandler(IReadRepository<SMMS.Domain.Entities.foodmenu.Menu , int> read,
                                    IWriteRepository<SMMS.Domain.Entities.foodmenu.Menu , int> write)
    {
        _read = read;
        _write = write;
    }

    public async Task<bool> Handle(UpdateMenuCommand request, CancellationToken ct)
    {
        var e = await _read.GetByIdAsync(
            request.Id,
            keyName: nameof(SMMS.Domain.Entities.foodmenu.Menu.MenuId),
            ct
        );
        if (e is null) return false;

        var d = request.Dto;
        e.PublishedAt = d.PublishedAt;
        e.SchoolId = d.SchoolId;
        e.IsVisible = d.IsVisible;
        e.WeekNo = d.WeekNo;
        e.ConfirmedBy = d.ConfirmedBy;
        e.ConfirmedAt = d.ConfirmedAt;
        e.AskToDelete = d.AskToDelete;
        e.YearId = d.YearId;

        await _write.UpdateAsync(e, ct);
        await _write.SaveChangeAsync(ct);
        return true;
    }
}
