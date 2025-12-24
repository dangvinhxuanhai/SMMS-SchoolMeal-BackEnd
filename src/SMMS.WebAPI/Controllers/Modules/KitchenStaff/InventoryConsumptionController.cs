using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SMMS.Application.Features.Inventory.Commands;
using SMMS.Application.Features.Inventory.DTOs;

namespace SMMS.WebAPI.Controllers.Modules.KitchenStaff;
[ApiController]
[Route("api/inventory")]
[Authorize(Roles = "KitchenStaff,Manager")]
public class InventoryConsumptionController : ControllerBase
{
    private readonly IMediator _mediator;

    public InventoryConsumptionController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Trừ kho theo actual usage của 1 tuần (ScheduleMeal)
    /// </summary>
    [HttpPost("consume-from-schedule/{scheduleMealId:long}")]
    public async Task<ActionResult<ConsumeInventoryResult>> Consume(
        long scheduleMealId,
        CancellationToken ct)
    {
        var userId = Guid.Parse(User.FindFirst("UserId")!.Value);

        var result = await _mediator.Send(
            new ConsumeInventoryFromScheduleCommand(
                scheduleMealId,
                userId),
            ct);

        return Ok(result);
    }
}

