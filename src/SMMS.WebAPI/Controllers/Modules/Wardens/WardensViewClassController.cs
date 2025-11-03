using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SMMS.Application.Features.Wardens.DTOs;
using SMMS.Application.Features.Wardens.Interfaces;

namespace SMMS.WebAPI.Controllers.Modules.Wardens;

[Route("api/[controller]")]
[ApiController]
public class WardensViewClassController : ControllerBase
{
    private readonly IWardensService _wardensService;

    public WardensViewClassController(IWardensService wardensService)
    {
        _wardensService = wardensService;
    }
    [HttpGet("classes/{classId}/search")]
    public async Task<IActionResult> Search(Guid classId, string keyword)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(keyword))
                return BadRequest(new { message = "Vui lòng nhập từ khóa tìm kiếm." });

            var result = await _wardensService.SearchAsync(classId, keyword);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
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
    // Lấy danh sách học sinh trong lớp
    [HttpGet("classes/{classId}/students")]
    public async Task<IActionResult> GetStudentsInClass(Guid classId)
    {
        try
        {
            var students = await _wardensService.GetStudentsInClassAsync(classId);
            return Ok(students);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    // Xuất báo cáo danh sách học sinh
    [HttpGet("classes/{classId}/export")]
    public async Task<IActionResult> ExportClass(Guid classId)
    {
        try
        {
            var reportData = await _wardensService.ExportClassStudentsAsync(classId);
            var fileName = $"class_report_{classId}_{DateTime.Now:yyyyMMdd}.xlsx";

            return File(reportData, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
}
