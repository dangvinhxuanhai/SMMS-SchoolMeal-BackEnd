using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SMMS.Application.Features.Manager.DTOs;
using SMMS.Application.Features.Manager.Interfaces;

namespace SMMS.WebAPI.Controllers.Modules.Manager;
[Route("api/[controller]")]
[ApiController]
public class ManagerParentController : ControllerBase
{
    private readonly IManagerParentService _service;

    public ManagerParentController(IManagerParentService service)
    {
        _service = service;
    }
    [HttpGet("search")]
    public async Task<IActionResult> Search(Guid schoolId, [FromQuery] string keyword)
    {
        if (string.IsNullOrWhiteSpace(keyword))
            return BadRequest(new { message = "Từ khóa tìm kiếm không được để trống." });

        var result = await _service.SearchAsync(schoolId, keyword);
        return Ok(new { count = result.Count, data = result });
    }
    [HttpGet]
    public async Task<IActionResult> GetAll(Guid schoolId, Guid? classId)
    {
        var parents = await _service.GetAllAsync(schoolId, classId);
        return Ok(new { count = parents.Count, data = parents });
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateParentRequest request)
    {
        var result = await _service.CreateAsync(request);
        return Ok(new { message = "Tạo tài khoản phụ huynh thành công!", data = result });
    }

    [HttpPut("{userId}")]
    public async Task<IActionResult> Update(Guid userId, [FromBody] UpdateParentRequest request)
    {
        var result = await _service.UpdateAsync(userId, request);
        if (result == null)
            return NotFound(new { message = "Không tìm thấy phụ huynh cần cập nhật." });

        return Ok(new { message = "Cập nhật thành công!", data = result });
    }

    [HttpPatch("{userId}/status")]
    public async Task<IActionResult> ChangeStatus(Guid userId, [FromQuery] bool isActive)
    {
        var success = await _service.ChangeStatusAsync(userId, isActive);
        if (!success)
            return NotFound(new { message = "Không tìm thấy tài khoản." });

        return Ok(new { message = "Cập nhật trạng thái thành công!" });
    }

    [HttpDelete("{userId}")]
    public async Task<IActionResult> Delete(Guid userId)
    {
        var success = await _service.DeleteAsync(userId);
        if (!success)
            return NotFound(new { message = "Không tìm thấy tài khoản." });

        return Ok(new { message = "Xóa tài khoản thành công!" });
    }
    [HttpPost("import-excel")]
    public async Task<IActionResult> ImportExcel(Guid schoolId, IFormFile file, [FromQuery] string createdBy)
    {
        if (file == null || file.Length == 0)
            return BadRequest(new { message = "Vui lòng chọn file Excel hợp lệ." });

        var result = await _service.ImportFromExcelAsync(schoolId, file, createdBy);
        return Ok(new
        {
            message = $"Đã nhập thành công phụ huynh từ file Excel.",
            data = result
        });
    }
    [HttpGet("download-template")]
    public async Task<IActionResult> DownloadTemplate()
    {
        var fileBytes = await _service.GetExcelTemplateAsync();

        return File(fileBytes,
            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            "Mau_Nhap_PhuHuynh.xlsx");
    }

}
