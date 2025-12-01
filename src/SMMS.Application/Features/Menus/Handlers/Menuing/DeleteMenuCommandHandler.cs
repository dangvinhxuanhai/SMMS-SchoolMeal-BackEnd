using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediatR;
using SMMS.Application.Common.Interfaces;
using SMMS.Application.Features.Menus.Command.Menuing;

namespace SMMS.Application.Features.Menus.Handlers.Menuing;
public sealed class DeleteMenuCommandHandler : IRequestHandler<DeleteMenuCommand, bool>
{
    private readonly IReadRepository<SMMS.Domain.Entities.foodmenu.Menu , int> _read;
    private readonly IWriteRepository<SMMS.Domain.Entities.foodmenu.Menu , int> _write;

    public DeleteMenuCommandHandler(IReadRepository<SMMS.Domain.Entities.foodmenu.Menu , int> read,
                                    IWriteRepository<SMMS.Domain.Entities.foodmenu.Menu , int> write)
    {
        _read = read;
        _write = write;
    }

    public async Task<bool> Handle(DeleteMenuCommand request, CancellationToken ct)
    {
        var exists = await _read.GetByIdAsync(
            request.Id,
            keyName: nameof(SMMS.Domain.Entities.foodmenu.Menu.MenuId),
            ct
        );
        if (exists is null) return false;

        await _write.DeleteByIdAsync(request.Id, ct);
        await _write.SaveChangeAsync(ct);
        return true;
    }
}
