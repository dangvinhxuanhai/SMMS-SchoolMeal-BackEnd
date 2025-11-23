using MediatR;
using Microsoft.AspNetCore.Mvc;
using SMMS.Application.Features.nutrition.Commands;
using SMMS.Application.Features.nutrition.DTOs;
using SMMS.Application.Features.nutrition.Queries;

namespace SMMS.WebAPI.Controllers.Modules.KitchenStaff;

[ApiController]
[Route("api/nutrition/[controller]")]
public class FoodItemsController : ControllerBase
{
    private readonly IMediator _mediator;

    public FoodItemsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    // GET api/nutrition/fooditems?schoolId=...&keyword=...
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<FoodItemDto>>> GetList(
        [FromQuery] Guid schoolId,
        [FromQuery] string? keyword)
    {
        var result = await _mediator.Send(new GetFoodItemsQuery(schoolId, keyword));
        return Ok(result);
    }

    // GET api/nutrition/fooditems/5
    [HttpGet("{id:int}")]
    public async Task<ActionResult<FoodItemDto>> GetById(int id)
    {
        var result = await _mediator.Send(new GetFoodItemByIdQuery(id));
        if (result == null) return NotFound();
        return Ok(result);
    }

    // POST api/nutrition/fooditems
    [HttpPost]
    public async Task<ActionResult<FoodItemDto>> Create([FromBody] CreateFoodItemCommand command)
    {
        // có thể set SchoolId, CreatedBy từ token tại đây
        var created = await _mediator.Send(command);
        return CreatedAtAction(nameof(GetById), new { id = created.FoodId }, created);
    }

    // PUT api/nutrition/fooditems/5
    [HttpPut("{id:int}")]
    public async Task<ActionResult<FoodItemDto>> Update(
        int id,
        [FromBody] UpdateFoodItemCommand command)
    {
        if (id != command.FoodId) return BadRequest("Id mismatch");

        var updated = await _mediator.Send(command);
        return Ok(updated);
    }

    // DELETE api/nutrition/fooditems/5
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(
        int id,
        [FromQuery] bool hardDeleteIfNoRelation = false)
    {
        await _mediator.Send(new DeleteFoodItemCommand
        {
            FoodId = id,
            HardDeleteIfNoRelation = hardDeleteIfNoRelation
        });
        return NoContent();
    }
}
