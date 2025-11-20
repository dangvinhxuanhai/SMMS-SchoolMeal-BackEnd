using System.Security.Claims;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SMMS.Application.Features.Manager.Commands;
using SMMS.Application.Features.Manager.DTOs;
using SMMS.Application.Features.Manager.Interfaces;
using SMMS.Application.Features.Manager.Queries;

namespace SMMS.WebAPI.Controllers.Modules.Manager;
[Route("api/[controller]")]
[ApiController]
[Authorize(Roles = "Manager")]
public class ManagerClassController : ControllerBase
{
    private readonly IMediator _mediator;

    public ManagerClassController(IMediator mediator)
    {
        _mediator = mediator;
    }
    private Guid GetSchoolIdFromToken()
    {
        var schoolIdClaim = User.FindFirst("SchoolId")?.Value;
        if (string.IsNullOrEmpty(schoolIdClaim))
            throw new UnauthorizedAccessException("Không tìm thấy SchoolId trong token.");

        return Guid.Parse(schoolIdClaim);
    }
    // 🟢 GET: /api/ManagerClass?schoolId={id}
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var schoolId = GetSchoolIdFromToken();
        var result = await _mediator.Send(new GetAllClassesQuery(schoolId));
        return Ok(new { count = result.Count, data = result });
    }

    // 🟡 POST: /api/ManagerClass
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateClassRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var result = await _mediator.Send(new CreateClassCommand(request));
        return Ok(result);
    }

    // 🟠 PUT: /api/ManagerClass/{id}
    [HttpPut("{classId:guid}")]
    public async Task<IActionResult> Update(Guid classId, [FromBody] UpdateClassRequest request)
    {
        var result = await _mediator.Send(new UpdateClassCommand(classId, request));
        if (result == null)
            return NotFound(new { message = "Không tìm thấy lớp để cập nhật." });

        return Ok(result);
    }

    // 🔴 DELETE: /api/ManagerClass/{id}
    [HttpDelete("{classId:guid}")]
    public async Task<IActionResult> Delete(Guid classId)
    {
        var success = await _mediator.Send(new DeleteClassCommand(classId));
        if (!success)
            return NotFound(new { message = "Không tìm thấy lớp để xóa." });

        return Ok(new { message = "Đã xóa lớp học thành công." });
    }

    // 🧑‍🏫 GET: /api/ManagerClass/teachers/assignment-status?schoolId={id}
    [HttpGet("teachers/assignment-status")]
    public async Task<IActionResult> GetTeacherAssignmentStatus()
    {
        try
        {
            var schoolId = GetSchoolIdFromToken();
            var result = await _mediator.Send(new GetTeacherAssignmentStatusQuery(schoolId));
            return Ok(result);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = $"Lỗi khi lấy danh sách giáo viên: {ex.Message}" });
        }
    }
}

