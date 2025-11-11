using System.Security.Claims;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.EntityFrameworkCore;
using SMMS.Application.Features.billing.Commands;
using SMMS.Application.Features.billing.DTOs;
using SMMS.Application.Features.billing.Queries;
using SMMS.Application.Features.notification.Interfaces;
using SMMS.Domain.Entities.auth;
using SMMS.Domain.Entities.billing;
using SMMS.Persistence.Data;

namespace SMMS.API.Controllers
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

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateNotificationDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var adminId = GetCurrentUserId();
            var result = await _mediator.Send(new CreateNotificationCommand(dto, adminId));

            return Ok(new { message = result });
        }

        [HttpGet("history")]
        public async Task<IActionResult> GetHistory()
        {
            var notifications = await _mediator.Send(new GetNotificationHistoryQuery());
            return Ok(notifications);
        }

        [HttpGet("{id:long}")]
        public async Task<IActionResult> GetById(long id)
        {
            var notification = await _mediator.Send(new GetNotificationByIdQuery(id));
            if (notification == null)
                return NotFound(new { message = "Không tìm thấy thông báo." });

            return Ok(notification);
        }

        private Guid GetCurrentUserId()
        {
            var idClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!Guid.TryParse(idClaim, out var adminId))
                throw new UnauthorizedAccessException("Token không hợp lệ.");
            return adminId;
        }
    }
}
