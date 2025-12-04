using System.Security.Claims;
using Azure.Core;
using DocumentFormat.OpenXml.Office2010.Excel;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using SMMS.Application.Features.foodmenu.DTOs;
using SMMS.Application.Features.Inventory.Commands;
using SMMS.Application.Features.Inventory.DTOs;
using SMMS.Application.Features.Inventory.Queries;
using SMMS.Application.Features.Meal.Command;
using SMMS.WebAPI.DTOs;

namespace SMMS.WebAPI.Controllers.Modules.KitchenStaff;
[ApiController]
[Route("api/inventory/[controller]")]
public class InventoryItemsController : ControllerBase
{
    private readonly IMediator _mediator;

    public InventoryItemsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    // GET: api/inventory/InventoryItems?pageIndex=1&pageSize=20
    [HttpGet]
    public async Task<ActionResult<PagedResult<InventoryItemDto>>> GetAll(
        [FromQuery] int pageIndex = 1,
        [FromQuery] int pageSize = 10,
        CancellationToken ct = default)
    {
        try
        {
            var schoolId = GetSchoolIdFromToken();

            var query = new GetInventoryItemsQuery(
                SchoolId: schoolId,
                PageIndex: pageIndex,
                PageSize: pageSize);

            var result = await _mediator.Send(query, ct);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    // GET: api/inventory/InventoryItems/5
    [HttpGet("{id:int}")]
    public async Task<ActionResult<InventoryItemDto>> GetById(
        int id,
        CancellationToken ct)
    {
        try
        {
            var schoolId = GetSchoolIdFromToken();

            var dto = await _mediator.Send(
                new GetInventoryItemByIdQuery(schoolId, id),
                ct);

            if (dto == null) return NotFound();

            return Ok(dto);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }


    // PUT: api/inventory/InventoryItems/5
    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(
        int id,
        [FromBody] UpdateInventoryItemRequest request,
        CancellationToken ct)
    {
        try
        {
            var schoolId = GetSchoolIdFromToken();

            var command = new UpdateInventoryItemCommand
            {
                SchoolId = schoolId,
                ItemId = id,
                QuantityGram = request.QuantityGram,
                ExpirationDate = request.ExpirationDate,
                BatchNo = request.BatchNo,
                Origin = request.Origin
            };
            await _mediator.Send(command, ct);
            return NoContent();
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    // ============ helpers lấy claim ============

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
}
