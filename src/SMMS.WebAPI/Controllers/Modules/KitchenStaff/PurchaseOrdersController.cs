using System.Security.Claims;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SMMS.Application.Features.Plan.Commands;
using SMMS.Application.Features.Plan.Queries;

namespace SMMS.WebAPI.Controllers.Modules.KitchenStaff;

[ApiController]
[Route("api/kitchen/purchase-orders")]
[Authorize(Roles = "kitchen_staff,manager,KitchenStaff,Manager")]
public class PurchaseOrdersController : ControllerBase
{
    private readonly IMediator _mediator;

    public PurchaseOrdersController(IMediator mediator)
    {
        _mediator = mediator;
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

    // POST: tạo PO + Lines từ Plan
    [HttpPost("from-plan")]
    public async Task<IActionResult> CreateFromPlan([FromBody] CreatePurchaseOrderFromPlanCommand body)
    {
        var staffId = body.StaffId == Guid.Empty ? GetCurrentUserId() : body.StaffId;

        var cmd = body with { StaffId = staffId };
        var result = await _mediator.Send(cmd);
        return Ok(result);
    }

    // GET list
    [HttpGet]
    public async Task<IActionResult> GetList(
        [FromQuery] Guid schoolId,
        [FromQuery] DateTime? fromDate,
        [FromQuery] DateTime? toDate)
    {
        var query = new GetPurchaseOrdersBySchoolQuery(schoolId, fromDate, toDate);
        var result = await _mediator.Send(query);
        return Ok(result);
    }

    // GET detail
    [HttpGet("{orderId:int}")]
    public async Task<IActionResult> GetById(int orderId, [FromQuery] Guid schoolId)
    {
        var query = new GetPurchaseOrderByIdQuery(orderId, schoolId);
        var result = await _mediator.Send(query);
        if (result == null) return NotFound();
        return Ok(result);
    }

    // PUT header
    [HttpPut("{orderId:int}")]
    public async Task<IActionResult> UpdateHeader(
        int orderId,
        [FromQuery] Guid schoolId,
        [FromBody] UpdatePurchaseOrderHeaderCommand body)
    {
        var cmd = body with { OrderId = orderId, SchoolId = schoolId };
        var result = await _mediator.Send(cmd);
        return Ok(result);
    }

    // DELETE order
    [HttpDelete("{orderId:int}")]
    public async Task<IActionResult> Delete(int orderId, [FromQuery] Guid schoolId)
    {
        var cmd = new DeletePurchaseOrderCommand(orderId, schoolId);
        await _mediator.Send(cmd);
        return NoContent();
    }

    // PUT lines
    [HttpPut("{orderId:int}/lines")]
    public async Task<IActionResult> UpdateLines(
        int orderId,
        [FromQuery] Guid schoolId,
        [FromBody] List<SMMS.Application.Features.Plan.DTOs.PurchaseOrderLineUpdateDto> lines)
    {
        var cmd = new UpdatePurchaseOrderLinesCommand(
            orderId,
            schoolId,
            GetCurrentUserId(),
            lines);

        var result = await _mediator.Send(cmd);
        return Ok(result);
    }

    // DELETE 1 line
    [HttpDelete("{orderId:int}/lines/{linesId:int}")]
    public async Task<IActionResult> DeleteLine(
        int orderId,
        int linesId,
        [FromQuery] Guid schoolId)
    {
        var cmd = new DeletePurchaseOrderLineCommand(orderId, linesId, schoolId);
        await _mediator.Send(cmd);
        return NoContent();
    }
}
