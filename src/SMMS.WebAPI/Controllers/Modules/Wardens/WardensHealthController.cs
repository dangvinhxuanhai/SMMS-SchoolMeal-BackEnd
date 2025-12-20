using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SMMS.Application.Features.Wardens.Commands;
using SMMS.Application.Features.Wardens.DTOs;
using SMMS.Application.Features.Wardens.Interfaces;
using SMMS.Application.Features.Wardens.Queries;
namespace SMMS.WebAPI.Controllers.Modules.Wardens;

[Route("api/[controller]")]
[ApiController]
[Authorize(Roles = "Teacher")]
public class WardensHealthController : ControllerBase
{
    private readonly IMediator _mediator;

    public WardensHealthController(IMediator mediator)
    {
        _mediator = mediator;
    }

    // 🩺 Lấy danh sách các chỉ số BMI mới nhất của học sinh trong lớp
    // GET: /api/WardensHealth/class/{classId}/health
    [HttpGet("class/{classId:guid}/health")]
    public async Task<IActionResult> GetHealthRecords1(Guid classId)
    {
        try
        {
            var healthData = await _mediator.Send(new GetStudentsHealthQuery(classId));
            return Ok(healthData);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    // 🔟 Xuất Excel báo cáo BMI học sinh
    // GET: /api/WardensHealth/class/{classId}/health/export
    [HttpGet("class/{classId:guid}/health/export")]
    public async Task<IActionResult> ExportHealthToExcel(Guid classId)
    {
        try
        {
            var reportData = await _mediator.Send(new ExportClassHealthQuery(classId));
            var fileName = $"BaoCao_SucKhoeLop_{classId}_{DateTime.Now:yyyyMMddHHmmss}.xlsx";

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

    // 📈 Lấy dữ liệu sức khỏe học sinh trong lớp (theo từng lần đo) cho chart
    // GET: /api/WardensHealth/class/{classId}/chart/health
    [HttpGet("class/{classId:guid}/chart/health")]
    public async Task<IActionResult> GetHealthRecords(Guid classId)
    {
        try
        {
            var healthData = await _mediator.Send(new GetStudentsHealthQuery(classId));
            return Ok(healthData);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
    // 🧾 1) Lịch sử BMI của 1 học sinh
    // GET: /api/WardensHealth/student/{studentId}/bmi-history
    [HttpGet("student/{studentId:guid}/bmi-history")]
    public async Task<IActionResult> GetStudentBmiHistory(Guid studentId)
    {
        try
        {
            var result = await _mediator.Send(new GetStudentBmiHistoryQuery(studentId));
            return Ok(result);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    // ➕ 2) Tạo record BMI mới cho học sinh
    // POST: /api/WardensHealth/student/{studentId}/bmi
    [HttpPost("student/{studentId:guid}/bmi")]
    public async Task<IActionResult> CreateStudentBmi(Guid studentId, [FromBody] CreateBmiRequest request)
    {
        try
        {
            var cmd = new CreateStudentBmiCommand(
                studentId,
                request.HeightCm,
                request.WeightKg,
                request.RecordDate
            );

            var result = await _mediator.Send(cmd);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    // ♻️ 3) Cập nhật 1 record BMI theo RecordId
    // PUT: /api/WardensHealth/bmi/{recordId}
    [HttpPut("bmi/{recordId:guid}")]
    public async Task<IActionResult> UpdateStudentBmi(Guid recordId, [FromBody] UpdateBmiRequest request)
    {
        try
        {
            var cmd = new UpdateStudentBmiCommand(
                recordId,
                request.HeightCm,
                request.WeightKg,
                request.RecordDate
            );

            var result = await _mediator.Send(cmd);
            if (result == null)
                return NotFound(new { message = "Health record not found" });

            return Ok(result);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    // ❌ 4) Xoá 1 record BMI
    // DELETE: /api/WardensHealth/bmi/{recordId}
    [HttpDelete("bmi/{recordId:guid}")]
    public async Task<IActionResult> DeleteStudentBmi(Guid recordId)
    {
        try
        {
            var ok = await _mediator.Send(new DeleteStudentBmiCommand(recordId));
            if (!ok)
                return NotFound(new { message = "Health record not found" });

            return NoContent();
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
}
