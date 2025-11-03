using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SMMS.Application.Features.Wardens.DTOs;
using SMMS.Application.Features.Wardens.Interfaces;

namespace SMMS.WebAPI.Controllers.Modules.Wardens;
[Route("api/[controller]")]
[ApiController]
public class WardensFeedbackController : ControllerBase
{
    private readonly IWardensFeedbackService _feedbackService;

    public WardensFeedbackController(IWardensFeedbackService feedbackService)
    {
        _feedbackService = feedbackService;
    }

    // 🟢 Lấy danh sách feedback của giám thị
    [HttpGet("{wardenId:guid}/list")]
    public async Task<IActionResult> GetFeedbacks(Guid wardenId)
    {
        try
        {
            var feedbacks = await _feedbackService.GetFeedbacksByWardenAsync(wardenId);

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
    [HttpPost("create")]
    public async Task<IActionResult> CreateFeedback([FromBody] CreateFeedbackRequest request)
    {
        try
        {
            var feedback = await _feedbackService.CreateFeedbackAsync(request);
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
}

