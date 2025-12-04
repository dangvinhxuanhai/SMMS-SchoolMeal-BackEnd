using System.Security.Claims;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SMMS.Application.Features.Wardens.DTOs;
using SMMS.Application.Features.Wardens.Interfaces;
using SMMS.Application.Features.Wardens.Queries;

namespace SMMS.WebAPI.Controllers.Modules.Wardens;

[Route("api/[controller]")]
[ApiController]
[Authorize(Roles = "Teacher")]
public class WardensViewClassController : ControllerBase
{
    private readonly IMediator _mediator;

    public WardensViewClassController(IMediator mediator)
    {
        _mediator = mediator;
    }

    private Guid GetCurrentWardenId()
    {
        var idClaim = User.FindFirst("UserId")?.Value ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value
            ?? User.FindFirst("sub")?.Value;
        if (string.IsNullOrEmpty(idClaim))
            throw new UnauthorizedAccessException("Không tìm thấy Warden ID trong token.");

        return Guid.Parse(idClaim);
    }

    // 🔍 Tìm kiếm học sinh/phụ huynh trong lớp
    // GET: /api/WardensViewClass/classes/{classId}/search?keyword=...
    [HttpGet("classes/{classId:guid}/search")]
    public async Task<IActionResult> Search(Guid classId, [FromQuery] string keyword)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(keyword))
                return BadRequest(new { message = "Vui lòng nhập từ khóa tìm kiếm." });

            var result = await _mediator.Send(new SearchStudentsInClassQuery(classId, keyword));
            return Ok(result);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    // 2️⃣ Lấy chi tiết điểm danh của một lớp
    // GET: /api/WardensViewClass/classes/{classId}/attendance
    [HttpGet("classes/{classId:guid}/attendance")]
    public async Task<IActionResult> GetClassAttendance(Guid classId)
    {
        try
        {
            var attendance = await _mediator.Send(new GetClassAttendanceQuery(classId));
            return Ok(attendance);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    // 🧒 Lấy danh sách học sinh trong lớp
    // GET: /api/WardensViewClass/classes/{classId}/students
    [HttpGet("classes/{classId:guid}/students")]
    public async Task<IActionResult> GetStudentsInClass(Guid classId)
    {
        try
        {
            var students = await _mediator.Send(new GetStudentsInClassQuery(classId));
            return Ok(students);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    // 📤 Xuất báo cáo danh sách học sinh
    // GET: /api/WardensViewClass/classes/{classId}/export
    [HttpGet("classes/{classId:guid}/export")]
    public async Task<IActionResult> ExportClass(Guid classId)
    {
        try
        {
            var reportData = await _mediator.Send(new ExportClassStudentsQuery(classId));
            var fileName = $"class_report_{classId}_{DateTime.Now:yyyyMMdd}.xlsx";

            return File(
                reportData,
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                fileName
            );
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
}
