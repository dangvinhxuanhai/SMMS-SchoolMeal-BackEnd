using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SMMS.Application.Features.Wardens.DTOs;
using SMMS.Application.Features.Wardens.Interfaces;
using SMMS.Application.Features.Wardens.Queries;
namespace SMMS.WebAPI.Controllers.Modules.Wardens;

[Route("api/[controller]")]
[ApiController]
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
            var healthData = await _mediator.Send(new GetHealthRecordsQuery(classId));
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
}
