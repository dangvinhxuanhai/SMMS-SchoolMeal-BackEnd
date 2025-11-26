using MediatR;
using Microsoft.AspNetCore.Mvc;
using SMMS.Application.Features.foodmenu.Queries;
using SMMS.Application.Features.Wardens.DTOs;

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

    /// <summary>
    /// Search / filter / sort feedbacks
    /// </summary>
    /// <remarks>
    /// Query params ví dụ:
    /// GET /api/Feedbacks?schoolId=...&keyword=com&fromCreatedAt=2025-11-01&sortBy=CreatedAt&sortDesc=true
    /// </remarks>
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<FeedbackDto>>> Search(
        [FromQuery] SearchFeedbacksQuery query)
    {
        var result = await _mediator.Send(query);
        return Ok(result);
    }

    /// <summary>
    /// Xem chi tiết 1 feedback
    /// </summary>
    [HttpGet("{id:int}")]
    public async Task<ActionResult<FeedbackDto>> GetById(int id)
    {
        var result = await _mediator.Send(new GetFeedbackByIdQuery(id));
        if (result == null) return NotFound();
        return Ok(result);
    }
}
