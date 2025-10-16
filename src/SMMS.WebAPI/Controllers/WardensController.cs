using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SMMS.Application.Features.Wardens.DTOs;
using SMMS.Application.Features.Wardens.Interfaces;

namespace SMMS.WebAPI.Controllers;

[Route("api/[controller]")]
[ApiController]
public class WardensController : ControllerBase
{
    private readonly IWardensService _wardensService;

    public WardensController(IWardensService wardensService)
    {
        _wardensService = wardensService;
    }

    // 2️⃣ Lấy chi tiết điểm danh của một lớp
    [HttpGet("classes/{classId}/attendance")]
    public async Task<IActionResult> GetClassAttendance(Guid classId)
    {
        try
        {
            var attendance = await _wardensService.GetClassAttendanceAsync(classId);
            return Ok(attendance);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    // 3️⃣ Xuất báo cáo điểm danh
    [HttpGet("classes/{classId}/attendance/export")]
    public async Task<IActionResult> ExportAttendanceReport(Guid classId)
    {
        try
        {
            var reportData = await _wardensService.ExportAttendanceReportAsync(classId);
            var fileName = $"attendance_report_{classId}_{DateTime.Now:yyyyMMdd}.xlsx";

            return File(reportData, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
}
