using System.Security.Claims;
using DocumentFormat.OpenXml.ExtendedProperties;
using DocumentFormat.OpenXml.Wordprocessing;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SMMS.Application.Features.Manager.DTOs;
using SMMS.Application.Features.Plan.Commands;
using SMMS.Application.Features.Plan.DTOs;
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

    // POST api/purchasing/PurchaseOrders/from-plan
    //api nay se tao purchase order de xem bill dong thoi cung update inventory,
    //trigger khi nguoi dung xac nhan da xong don hang purchase plan
    [HttpPost("from-plan")]
    public async Task<ActionResult<KsPurchaseOrderDto>> CreateFromPlan(
        [FromBody] CreatePurchaseOrderFromPlanDto request)
    {
        try
        {
            var command = new CreatePurchaseOrderFromPlanCommand
        {
            PlanId = request.PlanId,
            SupplierName = request.SupplierName,
            Note = request.Note,
            StaffUserId = GetCurrentUserId(),
            Lines = request.Lines
        };
        var result = await _mediator.Send(command);
        return CreatedAtAction(nameof(GetById), new { orderId = result.OrderId }, result);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message});
        }
    }

    // GET list
    [HttpGet]
    public async Task<IActionResult> GetList(
        [FromQuery] DateTime? fromDate,
        [FromQuery] DateTime? toDate)
    {
        var query = new GetPurchaseOrdersBySchoolQuery(GetSchoolIdFromToken(), fromDate, toDate);
        var result = await _mediator.Send(query);
        return Ok(result);
    }

    // GET detail
    [HttpGet("{orderId:int}")]
    public async Task<IActionResult> GetById(int orderId)
    {
        var query = new GetPurchaseOrderByIdQuery(orderId, GetSchoolIdFromToken());
        var result = await _mediator.Send(query);
        if (result == null) return NotFound();
        return Ok(result);
    }

    // PUT header
    [HttpPut("{orderId:int}")]
    public async Task<IActionResult> UpdateHeader(
        int orderId,
        [FromBody] UpdatePurchaseOrderHeaderCommand body)
    {
        try {
            var cmd = body with { OrderId = orderId, SchoolId = GetSchoolIdFromToken() };
            var result = await _mediator.Send(cmd);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message});
        }
    }

    // DELETE order
    [HttpDelete("{orderId:int}")]
    public async Task<IActionResult> Delete(int orderId)
    {
        try
        {
            var cmd = new DeletePurchaseOrderCommand(orderId, GetSchoolIdFromToken());
            await _mediator.Send(cmd);
            return NoContent();
        }
        catch (Exception ex) when (ex is InvalidOperationException || ex is ArgumentException || ex is KeyNotFoundException)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    // PUT lines
    [HttpPut("{orderId:int}/lines")]
    public async Task<IActionResult> UpdateLines(
        int orderId,
        [FromBody] List<SMMS.Application.Features.Plan.DTOs.PurchaseOrderLineUpdateDto> lines)
    {
        try
        {
            var cmd = new UpdatePurchaseOrderLinesCommand(
                orderId,
                GetSchoolIdFromToken(),
                GetCurrentUserId(),
                lines);

            var result = await _mediator.Send(cmd);
            return Ok(result);
        }
        catch (Exception ex) when (ex is InvalidOperationException || ex is ArgumentException || ex is KeyNotFoundException)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    // DELETE 1 line
    [HttpDelete("{orderId:int}/lines/{linesId:int}")]
    public async Task<IActionResult> DeleteLine(
        int orderId,
        int linesId)
    {
        try
        {
            var cmd = new DeletePurchaseOrderLineCommand(orderId, linesId, GetSchoolIdFromToken());
            await _mediator.Send(cmd);
            return NoContent();
        }
        catch (Exception ex) when (ex is InvalidOperationException || ex is ArgumentException || ex is KeyNotFoundException)
        {
            return BadRequest(new { error = ex.Message });
        }
    }
}
