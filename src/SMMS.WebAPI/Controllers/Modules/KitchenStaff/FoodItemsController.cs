using System.Security.Claims;
using Azure.Core;
using DocumentFormat.OpenXml.Office2016.Excel;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using SMMS.Application.Features.nutrition.Commands;
using SMMS.Application.Features.nutrition.DTOs;
using SMMS.Application.Features.nutrition.Queries;
using SMMS.Persistence;
using SMMS.Persistence.Service;

namespace SMMS.WebAPI.Controllers.Modules.KitchenStaff;

[ApiController]
[Route("api/nutrition/[controller]")]
public class FoodItemsController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly CloudinaryService _cloudinary;

    public FoodItemsController(IMediator mediator, CloudinaryService cloudinary)
    {
        _mediator = mediator;
        _cloudinary = cloudinary;
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
        try
        {
            var result = await _mediator.Send(new GetFoodItemsQuery(GetSchoolIdFromToken(), keyword));
            return Ok(result);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Get all food items, optionally filter by main dish or not.
    /// isMainDish = null: all, true: only main dishes, false: only side dishes.
    /// </summary>
    [HttpGet("by-main-dish")]
    public async Task<ActionResult<IReadOnlyList<FoodItemDto>>> GetByMainDish(
        [FromQuery] bool? isMainDish,
        [FromQuery] string? keyword,
        [FromQuery] bool includeInactive = false,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var query = new GetFoodItemsByMainDishQuery(
            GetSchoolIdFromToken(),
            isMainDish,
            keyword,
            includeInactive);

            var result = await _mediator.Send(query, cancellationToken);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
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
    [Consumes("multipart/form-data")]
    public async Task<ActionResult<FoodItemDto>> Create([FromForm] CreateFoodItemRequest request)
    {
        try
        {
            var schoolId = GetSchoolIdFromToken();
            var userId = GetCurrentUserId();

            string imageUrl = "";

            if (request.ImageFile != null)
            {
                var uploadedUrl = await _cloudinary.UploadImageAsync(request.ImageFile);
                if (!string.IsNullOrWhiteSpace(uploadedUrl))
                {
                    imageUrl = uploadedUrl;
                }
            }

            // 3. Tạo command gửi xuống Application
            var command = new CreateFoodItemCommand
            {
                SchoolId = schoolId,
                CreatedBy = userId,
                FoodName = request.FoodName,
                FoodType = request.FoodType,
                FoodDesc = request.FoodDesc,
                ImageUrl = imageUrl,
                IsMainDish = request.IsMainDish,
                Ingredients = request.Ingredients
            };
            var created = await _mediator.Send(command);
            return CreatedAtAction(nameof(GetById), new { id = created.FoodId }, created);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    // PUT api/nutrition/fooditems/5
    [HttpPut("{id:int}")]
    public async Task<ActionResult<FoodItemDto>> Update(
        int id,
        [FromBody] UpdateFoodItemCommand command)
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

    // DELETE api/nutrition/fooditems/5
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(
        int id,
        [FromQuery] bool hardDeleteIfNoRelation = false)
    {
        try
        {
            await _mediator.Send(new DeleteFoodItemCommand
            {
                FoodId = id,
                HardDeleteIfNoRelation = hardDeleteIfNoRelation
            });
            return NoContent();
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }
}
