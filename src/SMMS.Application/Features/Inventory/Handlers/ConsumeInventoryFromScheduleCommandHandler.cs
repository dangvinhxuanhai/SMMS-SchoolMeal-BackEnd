using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediatR;
using SMMS.Application.Features.Inventory.Commands;
using SMMS.Application.Features.Inventory.DTOs;
using SMMS.Application.Features.Inventory.Interfaces;

namespace SMMS.Application.Features.Inventory.Handlers;
public class ConsumeInventoryFromScheduleCommandHandler
    : IRequestHandler<ConsumeInventoryFromScheduleCommand, ConsumeInventoryResult>
{
    private readonly IInventoryConsumptionRepository _repo;

    public ConsumeInventoryFromScheduleCommandHandler(
        IInventoryConsumptionRepository repo)
    {
        _repo = repo;
    }

    public async Task<ConsumeInventoryResult> Handle(
        ConsumeInventoryFromScheduleCommand request,
        CancellationToken ct)
    {
        var items = await _repo.ConsumeByScheduleAsync(
            request.ScheduleMealId,
            request.ExecutedBy,
            ct);

        var overUsed = items.Where(x => x.IsOverConsumed).ToList();

        return new ConsumeInventoryResult
        {
            IsSuccess = true,
            Items = items,
            Warning = overUsed.Any()
                ? "Một số nguyên liệu đã bị dùng vượt tồn kho."
                : null
        };
    }
}
