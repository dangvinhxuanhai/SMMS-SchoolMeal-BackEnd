using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediatR;
using SMMS.Application.Common.Interfaces;
using SMMS.Application.Features.Menus.Command.MenuDaying;
using SMMS.Domain.Entities.foodmenu;

namespace SMMS.Application.Features.Menus.Handlers.MenuDaying;
public sealed class DeleteMenuDayCommandHandler : IRequestHandler<DeleteMenuDayCommand, bool>
{
    private readonly IReadRepository<MenuDay, int> _read;
    private readonly IWriteRepository<MenuDay, int> _write;

    public DeleteMenuDayCommandHandler(IReadRepository<MenuDay, int> read, IWriteRepository<MenuDay, int> write)
    {
        _read = read;
        _write = write;
    }

    public async Task<bool> Handle(DeleteMenuDayCommand request, CancellationToken ct)
    {
        var exists = await _read.GetByIdAsync(
            request.Id,
            keyName: nameof(MenuDay.MenuDayId),
            ct
        );
        if (exists is null) return false;

        await _write.DeleteByIdAsync(request.Id, ct);
        await _write.SaveChangeAsync(ct);
        return true;
    }
}
