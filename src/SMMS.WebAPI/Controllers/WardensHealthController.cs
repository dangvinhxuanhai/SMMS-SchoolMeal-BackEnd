using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SMMS.Application.Features.Wardens.DTOs;
using SMMS.Application.Features.Wardens.Interfaces;

namespace SMMS.WebAPI.Controllers;

[Route("api/[controller]")]
[ApiController]
public class WardensHealthController : ControllerBase
{
    private readonly IWardensService _wardensService;

    public WardensHealthController(IWardensService wardensService)
    {
        _wardensService = wardensService;
    }
    // lấy danh sách các chỉ số bmi của học sinh
     [HttpGet("class/{classId}/health")]
    public async Task<IActionResult> GetHealthRecords1(Guid classId)
    {
        try
        {
            var healthData = await _wardensService.GetHealthRecordsAsync(classId);
            return Ok(healthData);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    // 🔟 Xuất Excel báo cáo BMI học sinh
    [HttpGet("class/{classId}/health/export")]
    public async Task<IActionResult> ExportHealthToExcel(Guid classId)
    {
        try
        {
            var reportData = await _wardensService.ExportClassHealthAsync(classId);
            var fileName = $"BaoCao_SucKhoeLop_{classId}_{DateTime.Now:yyyyMMddHHmmss}.xlsx";

            return File(reportData, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }


    // Lấy biểu đồ  sức khỏe học sinh trong lớp ghi theo từng ngày
    [HttpGet("class/{classId}/chart/health")]
    public async Task<IActionResult> GetHealthRecords(Guid classId)
    {
        try
        {
            var healthData = await _wardensService.GetStudentsHealthAsync(classId);
            return Ok(healthData);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
}
