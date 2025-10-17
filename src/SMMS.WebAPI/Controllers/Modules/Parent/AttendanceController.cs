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
        public async Task<ActionResult<bool>> CreateAttendance([FromBody] CreateAttendanceRequestDto request)
        {
            try
            {
                // TẠM THỜI: Hardcode userId để test - sau này sẽ thay bằng authentication thật
                // Lấy parentId từ database seed (trong file seed data bạn cung cấp)
                var notifiedByUserId = Guid.Parse("7FF718D2-57CB-402C-8BCE-7681E0C0568D"); // Thay bằng parentId thực tế

                var result = await _attendanceService.CreateAttendanceAsync(request, notifiedByUserId);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet("student/{studentId}")]
        public async Task<ActionResult<AttendanceHistoryDto>> GetAttendanceByStudent(Guid studentId)
        {
            try
            {
                var history = await _attendanceService.GetAttendanceHistoryByStudentAsync(studentId);
                return Ok(history);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet("parent/{parentId}")]
        public async Task<ActionResult<AttendanceHistoryDto>> GetAttendanceByParent(Guid parentId)
        {
            try
            {
                var history = await _attendanceService.GetAttendanceHistoryByParentAsync(parentId);
                return Ok(history);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}
