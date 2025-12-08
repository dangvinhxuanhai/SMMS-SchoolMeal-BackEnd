using System.Security.Claims;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using SMMS.Application.Features.foodmenu.DTOs;
using SMMS.Application.Features.foodmenu.Queries;

namespace SMMS.WebAPI.Controllers.Modules.KitchenStaff;

[ApiController]
[Route("api/[controller]")]
public class FeedbacksController : ControllerBase
{
    private readonly IMediator _mediator;

    public FeedbacksController(IMediator mediator)
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
    /// Search / filter / sort feedbacks
    /// </summary>
    /// <remarks>
    /// Query params ví dụ:
    /// GET /api/Feedbacks?schoolId=...&keyword=com&fromCreatedAt=2025-11-01&sortBy=CreatedAt&sortDesc=true
    /// </remarks>
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<FeedbackKsDto>>> Search(
        [FromQuery] SearchFeedbacksQuery query)
    {
        try
        {
            query.SchoolId = GetSchoolIdFromToken();
            // query.SenderId = GetCurrentUserId();
            var result = await _mediator.Send(query);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Xem chi tiết 1 feedback
    /// </summary>
    [HttpGet("{id:int}")]
    public async Task<ActionResult<FeedbackKsDto>> GetById(int id)
    {
        try
        {
            var result = await _mediator.Send(new GetFeedbackByIdQuery(id));
            if (result == null) return NotFound();
            return Ok(result);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }
}
