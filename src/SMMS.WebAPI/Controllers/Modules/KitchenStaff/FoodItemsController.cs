using System.Security.Claims;
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

    // GET api/nutrition/fooditems?schoolId=...&keyword=...
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<FoodItemDto>>> GetList(
        [FromQuery] string? keyword)
    {
        var result = await _mediator.Send(new GetFoodItemsQuery(GetSchoolIdFromToken(), keyword));
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
        command.SchoolId = GetSchoolIdFromToken();
        command.CreatedBy = GetCurrentUserId();
        var created = await _mediator.Send(command);
        return CreatedAtAction(nameof(GetById), new { id = created.FoodId }, created);
    }

    // PUT api/nutrition/fooditems/5
    [HttpPut("{id:int}")]
    public async Task<ActionResult<FoodItemDto>> Update(
        int id,
        [FromBody] UpdateFoodItemCommand command)
    {
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
