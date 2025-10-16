using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SMMS.Application.Features.foodmenu.DTOs;
using SMMS.Application.Features.foodmenu.Queries;   // IWeeklyMenuQuery
using SMMS.Application.Features.foodmenu.Commands;

namespace SMMS.WebAPI.Controllers.Modules.Parent;

[ApiController]
[Route("api/parents/{studentId:guid}/meal-schedule")]
public sealed class ParentViewMealScheduleController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<ParentViewMealScheduleController> _logger;

    public ParentViewMealScheduleController(IMediator mediator, ILogger<ParentViewMealScheduleController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    [HttpGet("weeks")]
    [ProducesResponseType(typeof(IReadOnlyList<WeekOptionDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<WeekOptionDto>>> GetWeeks(
        Guid studentId,
        [FromQuery] DateOnly? from,
        [FromQuery] DateOnly? to,
        CancellationToken ct)
    {
        var data = await _mediator.Send(
            new GetAvailableWeeksQuery(studentId,
                from?.ToDateTime(TimeOnly.MinValue),
                to?.ToDateTime(TimeOnly.MinValue)), ct);

        return Ok(data);
    }

    [HttpGet]
    [ProducesResponseType(typeof(WeekMenuDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<WeekMenuDto>> GetWeekMenu(
        Guid studentId,
        [FromQuery] DateOnly date,
        CancellationToken ct)
    {
        var dto = await _mediator.Send(new GetWeekMenuQuery(studentId, date.ToDateTime(TimeOnly.MinValue)), ct);
        if (dto is null)
            return NotFound(new { message = "Không tìm thấy thực đơn tuần đã công bố cho ngày này." });
        return Ok(dto);
    }

    // (tuỳ chọn) gửi feedback
    //public sealed record SendFeedbackRequest(int DailyMealId, string Content);

    //[HttpPost("feedback")]
    //public async Task<IActionResult> SendFeedback(Guid studentId, [FromBody] SendFeedbackRequest req, CancellationToken ct)
    //{
    //    if (req.DailyMealId <= 0 || string.IsNullOrWhiteSpace(req.Content))
    //        return BadRequest(new { message = "Thiếu DailyMealId hoặc Content." });

    //    var senderId = GetUserIdFromClaims();
    //    if (senderId == Guid.Empty) return Forbid();

    //    var ok = await _mediator.Send(new SendMealFeedbackCommand(senderId, req.DailyMealId, req.Content.Trim()), ct);
    //    return ok ? Ok(new { message = "Đã gửi phản hồi." }) : BadRequest(new { message = "Gửi phản hồi thất bại." });
    //}

    //private Guid GetUserIdFromClaims()
    //{
    //    var id = User.FindFirstValue("sub") ?? User.FindFirstValue(ClaimTypes.NameIdentifier);
    //    return Guid.TryParse(id, out var g) ? g : Guid.Empty;
    //}
}
