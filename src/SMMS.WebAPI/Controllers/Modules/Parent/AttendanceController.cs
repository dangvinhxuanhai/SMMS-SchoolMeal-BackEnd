using Microsoft.AspNetCore.Mvc;
using SMMS.Application.Features.school.DTOs;
using SMMS.Application.Features.school.Interfaces;
using System;
using System.Threading.Tasks;

namespace SMMS.WebAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AttendanceController : ControllerBase
    {
        private readonly IAttendanceService _attendanceService;

        public AttendanceController(IAttendanceService attendanceService)
        {
            _attendanceService = attendanceService;
        }

        [HttpPost]
        public async Task<ActionResult> CreateAttendance([FromBody] AttendanceRequestDto request)
        {
            try
            {
                // Tạm thời lấy parentId cứng để test
                var parentId = Guid.Parse("3F75C8A8-F13B-44EA-B348-B50042547FAE"); // Parent thật của học sinh
                var result = await _attendanceService.CreateAttendanceAsync(request, parentId);
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

        [HttpGet("parent/{parentId}")]
        public async Task<ActionResult> GetByParent(Guid parentId)
        {
            var records = await _attendanceService.GetAttendanceHistoryByParentAsync(parentId);
            return Ok(records);
        }
    }
}
