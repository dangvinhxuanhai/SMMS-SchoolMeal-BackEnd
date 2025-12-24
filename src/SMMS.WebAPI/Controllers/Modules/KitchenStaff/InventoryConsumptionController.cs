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
        var userClaim = User.FindFirst("UserId") ?? User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
        if (userClaim == null)
        {
            return Unauthorized(new { message = "Không tìm thấy định danh người dùng trong Token" });
        }
        if (!Guid.TryParse(userClaim.Value, out Guid userId))
        {
            return BadRequest(new { message = "UserId không đúng định dạng Guid" });
        }
        var result = await _mediator.Send(
            new ConsumeInventoryFromScheduleCommand(
                scheduleMealId,
                userId),
            ct);

        return Ok(result);
    }
}

