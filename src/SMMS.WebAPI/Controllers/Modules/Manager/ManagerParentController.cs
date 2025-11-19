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
public class ManagerParentController : ControllerBase
{
    private readonly IMediator _mediator;

    public ManagerParentController(IMediator mediator)
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
    // 🔍 Tìm kiếm phụ huynh
    [HttpGet("search")]
    public async Task<IActionResult> Search([FromQuery] string keyword)
    {
        var schoolId = GetSchoolIdFromToken();
        if (string.IsNullOrWhiteSpace(keyword))
            return BadRequest(new { message = "Từ khóa tìm kiếm không được để trống." });

        var result = await _mediator.Send(new SearchParentsQuery(schoolId, keyword));
        return Ok(new { count = result.Count, data = result });
    }

    // 🟢 Lấy danh sách phụ huynh (theo trường / theo lớp)
    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] Guid? classId)
    {
        var schoolId = GetSchoolIdFromToken();
        var parents = await _mediator.Send(new GetParentsQuery(schoolId, classId));
        return Ok(new { count = parents.Count, data = parents });
    }

    // 🟡 Tạo tài khoản phụ huynh + con + gán lớp
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateParentRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var result = await _mediator.Send(new CreateParentCommand(request));
        return Ok(new { message = "Tạo tài khoản phụ huynh thành công!", data = result });
    }

    // 🟠 Cập nhật phụ huynh + con
    [HttpPut("{userId:guid}")]
    public async Task<IActionResult> Update(Guid userId, [FromBody] UpdateParentRequest request)
    {
        var result = await _mediator.Send(new UpdateParentCommand(userId, request));
        if (result == null)
            return NotFound(new { message = "Không tìm thấy phụ huynh cần cập nhật." });

        return Ok(new { message = "Cập nhật thành công!", data = result });
    }

    // 🔵 Đổi trạng thái kích hoạt
    [HttpPatch("{userId:guid}/status")]
    public async Task<IActionResult> ChangeStatus(Guid userId, [FromQuery] bool isActive)
    {
        var success = await _mediator.Send(new ChangeParentStatusCommand(userId, isActive));
        if (!success)
            return NotFound(new { message = "Không tìm thấy tài khoản." });

        return Ok(new { message = "Cập nhật trạng thái thành công!" });
    }

    // 🔴 Xóa tài khoản phụ huynh + con + lớp
    [HttpDelete("{userId:guid}")]
    public async Task<IActionResult> Delete(Guid userId)
    {
        var success = await _mediator.Send(new DeleteParentCommand(userId));
        if (!success)
            return NotFound(new { message = "Không tìm thấy tài khoản." });

        return Ok(new { message = "Xóa tài khoản thành công!" });
    }

    // 📥 Import phụ huynh từ Excel
    [HttpPost("import-excel")]
    public async Task<IActionResult> ImportExcel(
        IFormFile file,
        [FromQuery] string createdBy)
    {
        var schoolId = GetSchoolIdFromToken();
        if (file == null || file.Length == 0)
            return BadRequest(new { message = "Vui lòng chọn file Excel hợp lệ." });

        var result = await _mediator.Send(
            new ImportParentsFromExcelCommand(schoolId, file, createdBy));

        return Ok(new
        {
            message = "Đã nhập thành công phụ huynh từ file Excel.",
            data = result
        });
    }

    // 📄 Download mẫu Excel
    [HttpGet("download-template")]
    public async Task<IActionResult> DownloadTemplate()
    {
        var fileBytes = await _mediator.Send(new GetParentExcelTemplateQuery());

        return File(
            fileBytes,
            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            "Mau_Nhap_PhuHuynh.xlsx");
    }
}
