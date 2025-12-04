using System.Numerics;
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

    private Guid GetSchoolIdFromToken()
    {
        var schoolIdClaim = User.FindFirst("SchoolId")?.Value;
        if (string.IsNullOrEmpty(schoolIdClaim))
            throw new UnauthorizedAccessException("Không tìm thấy SchoolId trong token.");

        return Guid.Parse(schoolIdClaim);
    }

    private Guid GetCurrentUserId()
    {
        var userIdString = User.FindFirst("UserId")?.Value
                           ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                           ?? User.FindFirst("sub")?.Value
                           ?? User.FindFirst("id")?.Value
                           ?? throw new Exception("Token does not contain UserId.");

        return Guid.Parse(userIdString);
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
        try {
            var result = await _mediator.Send(
                new UpdatePurchasePlanCommand(
                    request.PlanId,
                    request.PlanStatus,
                    confirmedBy,
                    request.Lines));
            return Ok(result);
        }
        catch (Exception ex) when (ex is InvalidOperationException || ex is ArgumentException || ex is KeyNotFoundException)
        {
            return BadRequest(new { error = ex.Message });
        }

    }

    // DELETE api/purchase-plans/{planId}
    [HttpDelete("{planId:int}")]
    public async Task<IActionResult> Delete(int planId)
    {
        try {
        await _mediator.Send(new SoftDeletePurchasePlanCommand(planId));
        return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message});
        }
    }

    // CHỈ DÙNG CHO ADMIN: hard delete thật sự
    [HttpDelete("{planId:int}/hard")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> HardDelete(int planId)
    {
        try {
            await _mediator.Send(new DeletePurchasePlanCommand(planId));
            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message});
        }
    }

    // GET api/purchase-plans?schoolId=...&includeDeleted=false
    [HttpGet]
    public async Task<ActionResult<List<PurchasePlanListItemDto>>> GetAll(
        [FromQuery] bool includeDeleted = false)
    {
        try
        {
            var query = new GetPurchasePlansQuery(GetSchoolIdFromToken(), includeDeleted);
            var result = await _mediator.Send(query);
            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    // ====== NEW: GET BY DATE (tìm plan của tuần chứa ngày đó) ======
    // GET api/purchase-plans/by-date?schoolId=...&date=2025-11-28
    [HttpGet("by-date")]
    public async Task<ActionResult<PurchasePlanDto>> GetByDate(
        [FromQuery] DateOnly? date)
    {
        try {
        var day = date ?? DateOnly.FromDateTime(DateTime.UtcNow);

        var result = await _mediator.Send(
            new GetPurchasePlanByDateQuery(GetSchoolIdFromToken(), day));

        if (result == null)
            return NotFound();

        return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message});
        }
    }
}
