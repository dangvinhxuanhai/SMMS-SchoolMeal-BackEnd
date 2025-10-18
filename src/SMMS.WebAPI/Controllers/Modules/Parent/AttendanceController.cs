using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SMMS.Application.Features.school.DTOs;
using SMMS.Application.Features.school.Interfaces;
using System;
using System.Security.Claims;
using System.Threading.Tasks;

namespace SMMS.WebAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Parent")]
    public class AttendanceController : ControllerBase
    {
        private readonly IAttendanceService _attendanceService;

        public AttendanceController(IAttendanceService attendanceService)
        {
            _attendanceService = attendanceService;
        }
        private Guid GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null)
            {
                throw new UnauthorizedAccessException("Không tìm thấy ID người dùng trong token.");
            }

            return Guid.Parse(userIdClaim.Value);
        }
        [HttpPost]
        public async Task<ActionResult> CreateAttendance([FromBody] AttendanceRequestDto request)
        {
            try
            {
                var parentId = GetCurrentUserId();
                await _attendanceService.CreateAttendanceAsync(request, parentId);
                return Ok(new { message = "Tạo đơn nghỉ thành công." });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet("student/{studentId}")]
        public async Task<ActionResult> GetByStudent(Guid studentId)
        {
            var records = await _attendanceService.GetAttendanceHistoryByStudentAsync(studentId);
            return Ok(records);
        }

        [HttpGet("my")]
        public async Task<ActionResult> GetMyAttendances()
        {
            try
            {
                var parentId = GetCurrentUserId();
                var records = await _attendanceService.GetAttendanceHistoryByParentAsync(parentId);
                return Ok(records);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}
