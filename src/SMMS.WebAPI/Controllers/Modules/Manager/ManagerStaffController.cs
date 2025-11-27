using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SMMS.Application.Features.Manager.Commands;
using SMMS.Application.Features.Manager.DTOs;
using SMMS.Application.Features.Manager.Handlers;
using SMMS.Application.Features.Manager.Interfaces;
using SMMS.Application.Features.Manager.Queries;

namespace SMMS.WebAPI.Controllers.Modules.Manager;
[Route("api/[controller]")]
[ApiController]
[Authorize(Roles = "Manager")]
public class ManagerStaffController : ControllerBase
{
    private readonly IMediator _mediator;

    public ManagerStaffController(IMediator mediator)
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
    // 🔍 Search account
    [HttpGet("search")]
    public async Task<IActionResult> SearchAccounts([FromQuery] string keyword)
    {
        var schoolId = GetSchoolIdFromToken();
        var result = await _mediator.Send(new SearchAccountsQuery(schoolId, keyword));

        return Ok(new
        {
            count = result.Count,
            data = result
        });
    }

    // 🟢 GET: Lấy danh sách tài khoản staff (warden + kitchenStaff)
    [HttpGet("staff")]
    public async Task<IActionResult> GetAllStaff()
    {
        var schoolId = GetSchoolIdFromToken();
        var result = await _mediator.Send(new GetAllStaffQuery(schoolId));

        return Ok(new
        {
            count = result.Count,
            data = result
        });
    }

    /// 🧪 Filter by role
    [HttpGet("filter-by-role")]
    public async Task<IActionResult> FilterByRole([FromQuery] string role)
    {
        var schoolId = GetSchoolIdFromToken();
        if (string.IsNullOrWhiteSpace(role))
            return BadRequest(new { message = "Role không được để trống." });

        var result = await _mediator.Send(new FilterByRoleQuery(schoolId, role));

        return Ok(new
        {
            count = result.Count,
            data = result
        });
    }

    // 🟡 POST: Tạo tài khoản mới
    [HttpPost("create")]
    public async Task<IActionResult> CreateAccount([FromBody] CreateAccountRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        try
        {
            var schoolId =  GetSchoolIdFromToken();
            request.SchoolId = schoolId;

            var userIdStr = User.FindFirst("UserId")?.Value;
            if (Guid.TryParse(userIdStr, out var uid))
            {
                request.CreatedBy = uid;
            }
            var account = await _mediator.Send(new CreateAccountCommand(request));

            return Ok(new
            {
                message = "Tạo tài khoản thành công!",
                data = account
            });
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = $"Lỗi hệ thống: {ex.Message}" });
        }
    }

    // 🟠 PUT: Cập nhật thông tin tài khoản
    [HttpPut("{userId:guid}")]
    public async Task<IActionResult> UpdateAccount(Guid userId, [FromBody] UpdateAccountRequest request)
    {
        var updated = await _mediator.Send(new UpdateAccountCommand(userId, request));

        if (updated == null)
            return NotFound(new { message = "Không tìm thấy tài khoản để cập nhật." });

        return Ok(new
        {
            message = "Cập nhật tài khoản thành công!",
            data = updated
        });
    }

    // 🔵 PATCH: Đổi trạng thái kích hoạt
    [HttpPatch("{userId:guid}/status")]
    public async Task<IActionResult> ChangeStatus(Guid userId, [FromQuery] bool isActive)
    {
        var result = await _mediator.Send(new ChangeStatusCommand(userId, isActive));

        if (!result)
            return NotFound(new { message = "Không tìm thấy tài khoản." });

        return Ok(new
        {
            message = $"Đã {(isActive ? "kích hoạt" : "vô hiệu hóa")} tài khoản."
        });
    }

    // 🔴 DELETE: Xóa tài khoản
    [HttpDelete("{userId:guid}")]
    public async Task<IActionResult> DeleteAccount(Guid userId)
    {
        var deleted = await _mediator.Send(new DeleteAccountCommand(userId));

        if (!deleted)
            return NotFound(new { message = "Không tìm thấy tài khoản để xóa." });

        return Ok(new { message = "Đã xóa tài khoản thành công." });
    }
}
