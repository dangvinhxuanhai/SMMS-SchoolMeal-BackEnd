using System.Security.Claims;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SMMS.Application.Features.Wardens.Queries;

namespace SMMS.WebAPI.Controllers.Modules.KitchenStaff
{
    [Route("api/[controller]")]
    [ApiController]
    public class GetNotificationController : ControllerBase
    {
        private readonly IMediator _mediator;
        public GetNotificationController(IMediator mediator)
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
        [HttpGet("notifications")]
        public async Task<IActionResult> GetNotifications()
        {
            try
            {
                var kitchenStaff = GetCurrentUserId();
                var notifications = await _mediator.Send(
                    new GetWardenNotificationsQuery(kitchenStaff));

                return Ok(notifications);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}
