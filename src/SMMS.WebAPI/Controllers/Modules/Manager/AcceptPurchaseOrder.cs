using System.Security.Claims;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SMMS.Application.Features.Plan.Commands;
using SMMS.Application.Features.Plan.DTOs;
using SMMS.Application.Features.Plan.Queries;

namespace SMMS.WebAPI.Controllers.Modules.Manager;
[ApiController]
[Route("api/manager/AcceptPurchase")]
[Authorize(Roles = "manager,Manager")]
public class AcceptPurchaseOrder : ControllerBase
{
    private readonly IMediator _mediator;

    public AcceptPurchaseOrder(IMediator mediator)
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

    // POST: manager accept purchase order
    // Đổi trạng thái api/manager/AcceptPurchase/{id}/confirm -> Confirmed và nhập kho
    [HttpPost("{orderId:int}/confirm")]
    public async Task<IActionResult> Confirm(int orderId)
    {
        var cmd = new ConfirmPurchaseOrderCommand(
            orderId,
            GetSchoolIdFromToken(),
            GetCurrentUserId());

        var result = await _mediator.Send(cmd);
        return Ok(result);
    }

    // POST: api/manager/AcceptPurchase/{id}/Reject -> mark purchase order as Rejected (không nhập kho)
    [HttpPost("{orderId:int}/Reject")]
    [Authorize(Roles = "manager,Manager")]
    public async Task<IActionResult> Export(int orderId)
    {
        var cmd = new RejectPurchaseOrderCommand(
            orderId,
            GetSchoolIdFromToken(),
            GetCurrentUserId());

        var result = await _mediator.Send(cmd);
        return Ok(result);
    }
}
