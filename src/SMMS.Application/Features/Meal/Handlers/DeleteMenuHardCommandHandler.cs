using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediatR;
using SMMS.Application.Features.Meal.Command;
using SMMS.Application.Features.Meal.Interfaces;

namespace SMMS.Application.Features.Meal.Handlers;
public class DeleteMenuHardCommandHandler
    : IRequestHandler<DeleteMenuHardCommand>
{
    private readonly IMenuRepository _menuRepository;

    public DeleteMenuHardCommandHandler(IMenuRepository menuRepository)
    {
        _menuRepository = menuRepository;
    }

    public async Task Handle(DeleteMenuHardCommand request, CancellationToken cancellationToken)
    {
        var deleted = await _menuRepository.HardDeleteAsync(request.MenuId, cancellationToken);

        if (!deleted)
        {
            // tuỳ bạn: dùng custom NotFoundException hoặc tạm thời dùng KeyNotFoundException
            throw new KeyNotFoundException($"Menu {request.MenuId} not found");
        }
    }
}
