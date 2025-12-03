using System.Security.Claims;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SMMS.Application.Features.Wardens.Commands;
using SMMS.Application.Features.Wardens.DTOs;
using SMMS.Application.Features.Wardens.Interfaces;
using SMMS.Application.Features.Wardens.Queries;

namespace SMMS.WebAPI.Controllers.Modules.Wardens;
[Route("api/[controller]")]
[ApiController]
[Authorize(Roles = "Teacher")]
public class WardensFeedbackController : ControllerBase
{
    private readonly IMediator _mediator;

    public WardensFeedbackController(IMediator mediator)
    {
        _mediator = mediator;
    }
    private Guid GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
        if (userIdClaim == null)
            throw new UnauthorizedAccessException("Không tìm thấy ID người dùng trong token.");

        return Guid.Parse(userIdClaim.Value);
    }
    // 🟢 Lấy danh sách feedback của giám thị
    // GET: /api/WardensFeedback/{wardenId}/list
    [HttpGet("list")]
    public async Task<IActionResult> GetFeedbacks()
    {
        try
        {
            var wardenId = GetCurrentUserId();
            var feedbacks = await _mediator.Send(new GetWardenFeedbacksQuery(wardenId));

            if (!feedbacks.Any())
                return NotFound(new { message = "Chưa có phản hồi nào." });

            return Ok(new
            {
                message = $"Tìm thấy {feedbacks.Count()} phản hồi.",
                data = feedbacks
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = $"Lỗi khi lấy danh sách feedback: {ex.Message}" });
        }
    }

    // 🟡 Tạo feedback gửi kitchen staff
    // POST: /api/WardensFeedback/create
    [HttpPost("create")]
    public async Task<IActionResult> CreateFeedback([FromBody] CreateFeedbackRequest request)
    {
        try
        {
            request.SenderId = GetCurrentUserId();
            var feedback = await _mediator.Send(new CreateWardenFeedbackCommand(request));

            return Ok(new
            {
                message = "Gửi phản hồi thành công!",
                data = feedback
            });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = $"Lỗi khi gửi phản hồi: {ex.Message}" });
        }
    }

    [HttpPut("{feedbackId:int}")]
    public async Task<IActionResult> UpdateFeedback(
            int feedbackId,
            [FromBody] CreateFeedbackRequest request)
    {
        try
        {
            var result = await _mediator.Send(
                new UpdateWardenFeedbackCommand(feedbackId, request));

            return Ok(result);
        }
        catch (ArgumentException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return Forbid(ex.Message);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpDelete("{feedbackId:int}")]
    public async Task<IActionResult> DeleteFeedback(
           int feedbackId)
    {
        try
        {
            var wardenId = GetCurrentUserId();
            var ok = await _mediator.Send(
                new DeleteWardenFeedbackCommand(feedbackId, wardenId));

            if (!ok)
                return NotFound(new { message = "Không tìm thấy phản hồi." });

            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            return Forbid(ex.Message);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
}


