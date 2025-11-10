using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SMMS.Application.Features.school.Interfaces;
using System;
using System.Security.Claims;
using System.Threading.Tasks;

namespace SMMS.WebAPI.Controllers
{
    [Authorize(Roles = "Parent")]
    [ApiController]
    [Route("api/[controller]")]
    public class StudentHealthController : ControllerBase
    {
        private readonly IStudentHealthRepository _studentHealthService;

        public StudentHealthController(IStudentHealthRepository studentHealthService)
        {
            _studentHealthService = studentHealthService;
        }

        // ✅ API 1: BMI hiện tại
        [HttpGet("current/{studentId:guid}")]
        public async Task<IActionResult> GetCurrentBMI(Guid studentId)
        {
            var parentId = GetCurrentUserId();
            var result = await _studentHealthService.GetCurrentBMIAsync(studentId, parentId);

            if (result == null)
                return NotFound(new { message = "Chưa có dữ liệu sức khỏe." });

            return Ok(result);
        }

        // ✅ API 2: BMI các năm học trước (có thể chọn năm)
        [HttpGet("history/{studentId:guid}")]
        public async Task<IActionResult> GetBMIHistory(Guid studentId, [FromQuery] string? year = null)
        {
            var parentId = GetCurrentUserId();
            var result = await _studentHealthService.GetBMIByYearsAsync(studentId, parentId, year);
            return Ok(result);
        }

        // ✅ Lấy ParentId từ JWT
        private Guid GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);

            if (userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out var userId))
                throw new UnauthorizedAccessException("Không tìm thấy ID người dùng trong token.");

            return userId;
        }
    }
}
