using System.Security.Claims;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.EntityFrameworkCore;
using SMMS.Application.Features.billing.Commands;
using SMMS.Application.Features.billing.DTOs;
using SMMS.Application.Features.billing.Queries;
using SMMS.Domain.Entities.auth;
using SMMS.Domain.Entities.billing;
using SMMS.Persistence.Data;

namespace SMMS.WebAPI.Controllers.Modules.Admin
{
    [Authorize(Roles = "Admin")]
    [ApiController]
    [Route("api/[controller]")]
    public class NotificationsController : ControllerBase
    {
        private readonly IMediator _mediator;

        public NotificationsController(IMediator mediator)
        {
            _mediator = mediator;
        }

        /// <summary>
        /// Tạo thông báo mới và gửi realtime đến tất cả người dùng.
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateNotificationDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var adminId = GetCurrentUserId();

            // Gọi Mediator để tạo notification, handler sẽ gửi Realtime
            var result = await _mediator.Send(new CreateNotificationCommand(dto, adminId));

            // Trả về DTO để frontend hiển thị
            return Ok(result);
        }

        /// <summary>
        /// Lấy lịch sử tất cả thông báo của hệ thống
        /// </summary>
        [HttpGet("history")]
        public async Task<IActionResult> GetHistory()
        {
            var notifications = await _mediator.Send(new GetNotificationHistoryQuery());
            return Ok(notifications);
        }

        /// <summary>
        /// Lấy chi tiết thông báo theo Id
        /// </summary>
        [HttpGet("{id:long}")]
        public async Task<IActionResult> GetById(long id)
        {
            var notification = await _mediator.Send(new GetNotificationByIdQuery(id));
            if (notification == null)
                return NotFound(new { message = "Không tìm thấy thông báo." });

            return Ok(notification);
        }

        /// <summary>
        /// Lấy Id người dùng hiện tại từ token
        /// </summary>
        private Guid GetCurrentUserId()
        {
            var idClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!Guid.TryParse(idClaim, out var adminId))
                throw new UnauthorizedAccessException("Token không hợp lệ.");
            return adminId;
        }
    }
}
