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
        request.SchoolId = GetSchoolIdFromToken();

        try
        {
            var result = await _mediator.Send(new CreateClassCommand(request));
            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = $"Lỗi hệ thống: {ex.Message}" });
        }
    }

    // 🟠 PUT: /api/ManagerClass/{id}
    [HttpPut("{classId:guid}")]
    public async Task<IActionResult> Update(Guid classId, [FromBody] UpdateClassRequest request)
    {
        try
        {
            var result = await _mediator.Send(new UpdateClassCommand(classId, request));
            if (result == null)
                return NotFound(new { message = "Không tìm thấy lớp để cập nhật." });

            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Lỗi hệ thống." });
        }
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
    [HttpGet("academic-years")]
    public async Task<IActionResult> GetAcademicYears()
    {
        try
        {
            var schoolId = GetSchoolIdFromToken();
            var result = await _mediator.Send(new GetAcademicYearsQuery(schoolId));
            return Ok(result);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = $"Lỗi lấy danh sách niên khóa: {ex.Message}" });
        }
    }
    // 📅 GET: /api/ManagerClass/academic-years/{yearId}
    [HttpGet("academic-years/{yearId:int}")]
    public async Task<IActionResult> GetAcademicYearById(int yearId)
    {
        var result = await _mediator.Send(new GetAcademicYearByIdQuery(yearId));
        if (result == null)
            return NotFound(new { message = "Không tìm thấy niên khóa." });

        return Ok(result);
    }

    // 📅 POST: /api/ManagerClass/academic-years
    [HttpPost("academic-years")]
    public async Task<IActionResult> CreateAcademicYear([FromBody] CreateAcademicYearRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        try
        {
            request.SchoolId = GetSchoolIdFromToken();

            var result = await _mediator.Send(new CreateAcademicYearCommand(request));
            return Ok(new
            {
                message = "Tạo niên khóa thành công!",
                data = result
            });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = $"Lỗi hệ thống: {ex.Message}" });
        }
    }

    // 📅 PUT: /api/ManagerClass/academic-years/{yearId}
    [HttpPut("academic-years/{yearId:int}")]
    public async Task<IActionResult> UpdateAcademicYear(int yearId, [FromBody] UpdateAcademicYearRequest request)
    {
        try
        {
            var result = await _mediator.Send(new UpdateAcademicYearCommand(yearId, request));
            if (result == null)
                return NotFound(new { message = "Không tìm thấy niên khóa cần cập nhật." });

            return Ok(new
            {
                message = "Cập nhật niên khóa thành công!",
                data = result
            });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = $"Lỗi hệ thống: {ex.Message}" });
        }
    }

    // 📅 DELETE: /api/ManagerClass/academic-years/{yearId}
    [HttpDelete("academic-years/{yearId:int}")]
    public async Task<IActionResult> DeleteAcademicYear(int yearId)
    {
        var success = await _mediator.Send(new DeleteAcademicYearCommand(yearId));
        if (!success)
            return NotFound(new { message = "Không tìm thấy niên khóa cần xóa." });

        return Ok(new { message = "Xóa niên khóa thành công!" });
    }
}
