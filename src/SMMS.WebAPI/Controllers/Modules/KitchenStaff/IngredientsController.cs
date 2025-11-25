using MediatR;
using Microsoft.AspNetCore.Mvc;
using SMMS.Application.Features.nutrition.Commands;
using SMMS.Application.Features.nutrition.DTOs;
using SMMS.Application.Features.nutrition.Queries;

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

    /// <summary>
    /// Lấy danh sách Ingredient đang active (IsActive = 1)
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<IngredientDto>>> GetActive(
        [FromQuery] Guid schoolId,
        [FromQuery] string? keyword)
    {
        var result = await _mediator.Send(
            new GetIngredientsQuery(schoolId, keyword, IncludeInactive: false));

        return Ok(result);
    }

    /// <summary>
    /// Lấy tất cả Ingredient (kể cả đã soft delete)
    /// Thích hợp cho Admin.
    /// </summary>
    [HttpGet("all")]
    public async Task<ActionResult<IReadOnlyList<IngredientDto>>> GetAll(
        [FromQuery] Guid schoolId,
        [FromQuery] string? keyword)
    {
        var result = await _mediator.Send(
            new GetIngredientsQuery(schoolId, keyword, IncludeInactive: true));

        return Ok(result);
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
        // Có thể set SchoolId / CreatedBy từ Claims ở đây nếu muốn
        var created = await _mediator.Send(command);
        return CreatedAtAction(nameof(GetById), new { id = created.IngredientId }, created);
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<IngredientDto>> Update(
        int id,
        [FromBody] UpdateIngredientCommand command)
    {
        if (id != command.IngredientId)
            return BadRequest("Id mismatch");

        var updated = await _mediator.Send(command);
        return Ok(updated);
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
        await _mediator.Send(new DeleteIngredientCommand
        {
            IngredientId = id,
            HardDelete = hardDelete
        });

        return NoContent();
    }
}
