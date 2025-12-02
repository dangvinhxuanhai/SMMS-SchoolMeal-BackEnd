using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SMMS.Application.Features.school.Commands;
using SMMS.Application.Features.school.DTOs;
using SMMS.Application.Features.school.Queries;
using SMMS.Application.Features.Wardens.Queries;
using System;
using System.Security.Claims;
using System.Threading.Tasks;

namespace SMMS.WebAPI.Controllers.Modules.Parent
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Parent")]
    public class AttendanceController : ControllerBase
    {
        private readonly IMediator _mediator;

        public AttendanceController(IMediator mediator)
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
        public async Task<IActionResult> CreateAttendance([FromBody] AttendanceRequestDto request)
        {
            try
            {
                var parentId = GetCurrentUserId();
                var command = new CreateAttendanceCommand(request, parentId);
                await _mediator.Send(command);

                return Ok(new { message = "Tạo đơn nghỉ thành công." });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet("student/{studentId}")]
        public async Task<IActionResult> GetByStudent(Guid studentId)
        {
            var query = new GetAttendanceByStudentQuery(studentId);
            var result = await _mediator.Send(query);
            return Ok(result);
        }

        [HttpGet("my")]
        public async Task<IActionResult> GetMyAttendances()
        {
            try
            {
                var parentId = GetCurrentUserId();
                var query = new GetAttendanceByParentQuery(parentId);
                var result = await _mediator.Send(query);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
        [HttpGet("notifications")]
        public async Task<IActionResult> GetNotifications()
        {
            try
            {
                var parentId = GetCurrentUserId();
                var notifications = await _mediator.Send(
                    new GetWardenNotificationsQuery(parentId));

                return Ok(notifications);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}
