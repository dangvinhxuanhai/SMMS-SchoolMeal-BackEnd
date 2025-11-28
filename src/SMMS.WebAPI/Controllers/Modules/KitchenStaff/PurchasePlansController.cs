using System.Security.Claims;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SMMS.Application.Features.Plan.Commands;
using SMMS.Application.Features.Plan.DTOs;
using SMMS.Application.Features.Plan.Queries;
using SMMS.WebAPI.DTOs;

namespace SMMS.WebAPI.Controllers.Modules.KitchenStaff;
[ApiController]
[Route("api/purchase-plans")]
[Authorize]
public class PurchasePlansController : ControllerBase
{
    private readonly IMediator _mediator;

    public PurchasePlansController(IMediator mediator)
    {
        _mediator = mediator;
    }

    private Guid GetCurrentUserId()
    {
        // tuỳ hệ thống claim của anh
        var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier)
                       ?? throw new InvalidOperationException("UserId claim not found.");
        return Guid.Parse(userIdStr);
    }

    // POST api/purchase-plans/from-schedule?scheduleMealId=123
    [HttpPost("from-schedule")]
    public async Task<ActionResult<PurchasePlanDto>> CreateFromSchedule(
        [FromQuery] long scheduleMealId)
    {
        var staffId = GetCurrentUserId();

        var result = await _mediator.Send(
            new CreatePurchasePlanFromScheduleCommand(scheduleMealId, staffId));

        return CreatedAtAction(
            nameof(GetById),
            new { planId = result.PlanId },
            result);
    }

    // GET api/purchase-plans/{planId}
    [HttpGet("{planId:int}")]
    public async Task<ActionResult<PurchasePlanDto>> GetById(int planId)
    {
        var dto = await _mediator.Send(new GetPurchasePlanDetailQuery(planId));
        if (dto == null)
            return NotFound();

        return Ok(dto);
    }

    // PUT api/purchase-plans/{planId}
    [HttpPut("{planId:int}")]
    public async Task<ActionResult<PurchasePlanDto>> Update(
        int planId,
        [FromBody] UpdatePurchasePlanRequest request)
    {
        if (planId != request.PlanId)
            return BadRequest("PlanId mismatch.");

        Guid? confirmedBy = null;
        if (string.Equals(request.PlanStatus, "Confirmed", StringComparison.OrdinalIgnoreCase))
        {
            confirmedBy = GetCurrentUserId();
        }

        var result = await _mediator.Send(
            new UpdatePurchasePlanCommand(
                request.PlanId,
                request.PlanStatus,
                confirmedBy,
                request.Lines));

        return Ok(result);
    }

    // DELETE api/purchase-plans/{planId}
    [HttpDelete("{planId:int}")]
    public async Task<IActionResult> Delete(int planId)
    {
        await _mediator.Send(new DeletePurchasePlanCommand(planId));
        return NoContent();
    }

    // GET api/purchase-plans?schoolId=...&includeDeleted=false
    [HttpGet]
    public async Task<ActionResult<List<PurchasePlanListItemDto>>> GetAll(
        [FromQuery] Guid schoolId,
        [FromQuery] bool includeDeleted = false)
    {
        var query = new GetPurchasePlansQuery(schoolId, includeDeleted);
        var result = await _mediator.Send(query);
        return Ok(result);
    }

    // ====== NEW: GET BY DATE (tìm plan của tuần chứa ngày đó) ======
    // GET api/purchase-plans/by-date?schoolId=...&date=2025-11-28
    [HttpGet("by-date")]
    public async Task<ActionResult<PurchasePlanDto>> GetByDate(
        [FromQuery] Guid schoolId,
        [FromQuery] DateOnly? date)
    {
        var day = date ?? DateOnly.FromDateTime(DateTime.UtcNow);

        var result = await _mediator.Send(
            new GetPurchasePlanByDateQuery(schoolId, day));

        if (result == null)
            return NotFound();

        return Ok(result);
    }
}
