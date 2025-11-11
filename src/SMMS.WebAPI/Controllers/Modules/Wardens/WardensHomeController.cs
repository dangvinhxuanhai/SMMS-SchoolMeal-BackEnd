using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SMMS.Application.Features.Wardens.DTOs;
using SMMS.Application.Features.Wardens.Interfaces;
using SMMS.Application.Features.Wardens.Queries;

namespace SMMS.WebAPI.Controllers.Modules.Wardens;

[Route("api/[controller]")]
[ApiController]
public class WardensHomeController : ControllerBase
{
    private readonly IMediator _mediator;

    public WardensHomeController(IMediator mediator)
    {
        _mediator = mediator;
    }

    // 1️⃣ Lấy danh sách lớp mà giám thị phụ trách
    // GET: /api/WardensHome/classes/{wardenId}
    [HttpGet("classes/{wardenId:guid}")]
    public async Task<IActionResult> GetClasses(Guid wardenId)
    {
        try
        {
            var classes = await _mediator.Send(new GetWardenClassesQuery(wardenId));
            return Ok(classes);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    // 2️⃣ Lấy chi tiết điểm danh của một lớp
    // GET: /api/WardensHome/classes/{classId}/attendance
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

    // 3️⃣ Xuất báo cáo điểm danh
    // GET: /api/WardensHome/classes/{classId}/attendance/export
    [HttpGet("classes/{classId:guid}/attendance/export")]
    public async Task<IActionResult> ExportAttendanceReport(Guid classId)
    {
        try
        {
            var reportData = await _mediator.Send(new ExportAttendanceReportQuery(classId));
            var fileName = $"attendance_report_{classId}_{DateTime.Now:yyyyMMdd}.xlsx";

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

    // 4️⃣ Lấy thông báo của giám thị
    // GET: /api/WardensHome/notifications/{wardenId}
    [HttpGet("notifications/{wardenId:guid}")]
    public async Task<IActionResult> GetNotifications(Guid wardenId)
    {
        try
        {
            var notifications = await _mediator.Send(new GetWardenNotificationsQuery(wardenId));
            return Ok(notifications);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
}
