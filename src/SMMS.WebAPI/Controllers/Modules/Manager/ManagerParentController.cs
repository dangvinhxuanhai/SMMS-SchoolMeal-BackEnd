using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
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
    private readonly IManagerAccountRepository _accountRepo;   // 👈 thêm

    public ManagerParentController(IMediator mediator, IManagerAccountRepository accountRepo)
    {
        _mediator = mediator;
        _accountRepo = accountRepo;
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
    [Route("create-parent")]
    public async Task<IActionResult> Create([FromBody] CreateParentRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        try
        {
            request.SchoolId = GetSchoolIdFromToken();

            // 👉 Check trước xem có phụ huynh nào trùng email/phone trong hệ thống không
            var normalizedEmail = string.IsNullOrWhiteSpace(request.Email)
                ? null
                : request.Email.Trim().ToLower();

            var existingParent = await _accountRepo.Users
                .FirstOrDefaultAsync(u =>
                        ((normalizedEmail != null && u.Email == normalizedEmail) ||
                         u.Phone == request.Phone));

            bool isExistingParent = existingParent != null;

            // Gọi handler như cũ
            var result = await _mediator.Send(new CreateParentCommand(request));

            var message = isExistingParent
                ? "Phụ huynh đã tồn tại trong hệ thống. Hệ thống sử dụng lại thông tin phụ huynh và chỉ thêm con tại trường này."
                : "Tạo tài khoản phụ huynh thành công!";

            return Ok(new { message, data = result });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Lỗi hệ thống: " + ex.Message });
        }
    }


    // 🟠 Cập nhật phụ huynh + con
    [HttpPut("{userId:guid}")]
    public async Task<IActionResult> Update(Guid userId, [FromBody] UpdateParentRequest request)
    {
        var result = await _mediator.Send(new UpdateParentCommand(userId, request));
        if (result == null)
            return NotFound(new { message = "Mật khẩu đã được thay đổi không thể cập nhật thông tin phụ huynh." });

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
    // 🔴 Xóa phụ huynh KHỎI TRƯỜNG HIỆN TẠI (chỉ xóa con + lớp của trường này)
    [HttpDelete("{userId:guid}")]
    public async Task<IActionResult> Delete(Guid userId)
    {
        // Lấy SchoolId từ token (manager đang thuộc trường nào)
        var schoolId = GetSchoolIdFromToken();

        // Command mới: DeleteParentCommand(Guid userId, Guid schoolId)
        var success = await _mediator.Send(new DeleteParentCommand(userId, schoolId));

        if (!success)
            return NotFound(new { message = "Không tìm thấy tài khoản hoặc không có học sinh thuộc trường này." });

        return Ok(new { message = "Xóa phụ huynh khỏi trường hiện tại thành công!" });
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
