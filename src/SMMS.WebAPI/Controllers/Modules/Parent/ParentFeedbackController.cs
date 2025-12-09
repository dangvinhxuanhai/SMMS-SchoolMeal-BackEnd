using System.Security.Claims;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SMMS.Application.Features.foodmenu.Commands;
using SMMS.Application.Features.foodmenu.DTOs;
using SMMS.Application.Features.foodmenu.Queries;

namespace SMMS.WebAPI.Controllers.Modules.Parent
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Parent")]
    public class ParentFeedbackController : ControllerBase
    {
        private readonly IMediator _mediator;

        public ParentFeedbackController(IMediator mediator)
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
        [HttpPost]
        public async Task<IActionResult> CreateFeedback([FromBody] CreateFeedbackRequestDto requestDto)
        {
            var senderId = GetCurrentUserId();

            var dto = new CreateFeedbackDto(
                SenderId: senderId,
                Rating : requestDto.Rating,
                TargetType: "Meal",
                TargetRef: requestDto.DailyMealId.HasValue ? $"DailyMeal-{requestDto.DailyMealId.Value}" : null,
                Content: requestDto.Content,
                DailyMealId: requestDto.DailyMealId
            );

            var result = await _mediator.Send(new CreateFeedbackCommand(dto));
            return Ok(result);
        }

        [HttpGet("my-feedbacks")]
        public async Task<IActionResult> GetMyFeedbacks()
        {
            var senderId = GetCurrentUserId();
            var result = await _mediator.Send(new GetFeedbackBySenderQuery(senderId));
            return Ok(result);
        }
    }

}
