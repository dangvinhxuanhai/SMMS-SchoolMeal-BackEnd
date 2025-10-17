using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SMMS.Application.Features.school.DTOs;
using SMMS.Application.Features.school.Interfaces;
using System;
using System.Linq;
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

        // 🧾 Gửi đơn xin nghỉ học
        [HttpPost]
        public async Task<ActionResult> CreateAttendance([FromBody] AttendanceRequestDto request)
        {
            try
            {
                // ✅ Lấy ParentId từ token
                var userIdClaim = User.FindFirst("UserId");
                if (userIdClaim == null)
                    return Unauthorized(new { message = "Token không hợp lệ hoặc thiếu UserId." });

                var parentId = Guid.Parse(userIdClaim.Value);

                var result = await _attendanceService.CreateAttendanceAsync(request, parentId);
                return Ok(new { message = "Tạo đơn nghỉ thành công." });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // 🧒 Lịch sử đơn nghỉ theo học sinh
        [HttpGet("student/{studentId}")]
        public async Task<ActionResult> GetByStudent(Guid studentId)
        {
            var records = await _attendanceService.GetAttendanceHistoryByStudentAsync(studentId);
            return Ok(records);
        }

        // 👨‍👩‍👧 Lịch sử đơn nghỉ của chính phụ huynh đăng nhập
        [HttpGet("my")]
        public async Task<ActionResult> GetMyAttendances()
        {
            try
            {
                var userIdClaim = User.FindFirst("UserId");
                if (userIdClaim == null)
                    return Unauthorized(new { message = "Token không hợp lệ hoặc thiếu UserId." });

                var parentId = Guid.Parse(userIdClaim.Value);
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
