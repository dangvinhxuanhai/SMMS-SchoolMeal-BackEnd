using System.Security.Claims;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SMMS.Application.Features.Manager.Commands;
using SMMS.Application.Features.Wardens.DTOs;
using SMMS.Application.Features.Wardens.Interfaces;
using SMMS.Application.Features.Wardens.Queries;

namespace SMMS.WebAPI.Controllers.Modules.Wardens;

[Route("api/[controller]")]
[ApiController]
[Authorize(Roles = "Teacher")]
public class WardensHomeController : ControllerBase
{
    private readonly IMediator _mediator;

    public WardensHomeController(IMediator mediator)
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
    // 1️⃣ Lấy danh sách lớp mà giám thị phụ trách
    // GET: /api/WardensHome/classes/{wardenId}
    [HttpGet("classes")]
    public async Task<IActionResult> GetClasses()
    {
        try
        {
            var wardenId = GetCurrentUserId();
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
    [HttpGet("notifications")]
    public async Task<IActionResult> GetNotifications()
    {
        try
        {
            var wardenId = GetCurrentUserId();
            var notifications = await _mediator.Send(
                new GetWardenNotificationsQuery(wardenId));

            return Ok(notifications);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPut("read-all")]
    public async Task<IActionResult> MarkAllAsRead()
    {
        try
        {
            var userId = GetCurrentUserId();
            await _mediator.Send(new MarkAllNotificationsAsReadCommand(userId));

            return Ok(new { message = "Đã đánh dấu tất cả là đã đọc." });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    // 5️⃣ Lấy dữ liệu thống kê Dashboard (Tổng quan)
    // GET: /api/WardensHome/dashboard
    [HttpGet("dashboard")]
    public async Task<IActionResult> GetDashboardStats()
    {
        try
        {
            var wardenId = GetCurrentUserId();
            // Gọi Query đã được định nghĩa ở Region 7 trong Handler
            var dashboardData = await _mediator.Send(new GetWardenDashboardQuery(wardenId));
            return Ok(dashboardData);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
}
