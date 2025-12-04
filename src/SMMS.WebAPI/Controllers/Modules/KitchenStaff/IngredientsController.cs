using System.Security.Claims;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using SMMS.Application.Features.nutrition.Commands;
using SMMS.Application.Features.nutrition.DTOs;
using SMMS.Application.Features.nutrition.Queries;
using SMMS.Domain.Entities.school;

namespace SMMS.WebAPI.Controllers.Modules.KitchenStaff;
[ApiController]
[Route("api/nutrition/[controller]")]
public class IngredientsController : ControllerBase
{
    private readonly IMediator _mediator;

    public IngredientsController(IMediator mediator)
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

    /// <summary>
    /// Lấy danh sách Ingredient đang active (IsActive = 1)
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<IngredientDto>>> GetActive(
        [FromQuery] string? keyword)
    {
        try
        {
            var result = await _mediator.Send(
                new GetIngredientsQuery(GetSchoolIdFromToken(), keyword, IncludeInactive: false));

            return Ok(result);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Lấy tất cả Ingredient (kể cả đã soft delete)
    /// Thích hợp cho Admin.
    /// </summary>
    [HttpGet("all")]
    public async Task<ActionResult<IReadOnlyList<IngredientDto>>> GetAll(
        [FromQuery] string? keyword)
    {
        try
        {
            var result = await _mediator.Send(
            new GetIngredientsQuery(GetSchoolIdFromToken(), keyword, IncludeInactive: true));

            return Ok(result);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<IngredientDto>> GetById(int id)
    {
        var result = await _mediator.Send(new GetIngredientByIdQuery(id));
        if (result == null) return NotFound();
        return Ok(result);
    }

    [HttpPost]
    public async Task<ActionResult<IngredientDto>> Create([FromBody] CreateIngredientCommand command)
    {
        try
        {
            command.SchoolId = GetSchoolIdFromToken();
            command.CreatedBy = GetCurrentUserId();
            var created = await _mediator.Send(command);
            return CreatedAtAction(nameof(GetById), new { id = created.IngredientId }, created);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<IngredientDto>> Update(
        int id,
        [FromBody] UpdateIngredientCommand command)
    {
        try
        {
            var updated = await _mediator.Send(command);
            return Ok(updated);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Delete Ingredient.
    /// - Mặc định soft delete (IsActive = 0)
    /// - Nếu hardDelete = true → xóa bản ghi ở bảng liên quan rồi mới xóa Ingredient
    /// </summary>
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(
        int id,
        [FromQuery] bool hardDelete = false)
    {
        try
        {
            await _mediator.Send(new DeleteIngredientCommand
            {
                IngredientId = id,
                HardDelete = hardDelete
            });

            return NoContent();
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }
}
