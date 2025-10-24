using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SMMS.Application.Features.Manager.DTOs;
using SMMS.Application.Features.Manager.Handlers;
using SMMS.Application.Features.Manager.Interfaces;

namespace SMMS.WebAPI.Controllers.Modules.Manager;
[Route("api/[controller]")]
[ApiController]
public class ManagerStaffController : ControllerBase
{
    private readonly IManagerAccountService _accountService;

    public ManagerStaffController(IManagerAccountService accountService)
    {
        _accountService = accountService;
    }

    [HttpGet("search")]
    public async Task<IActionResult> SearchAccounts(Guid schoolId, [FromQuery] string keyword)
    {
        var result = await _accountService.SearchAccountsAsync(schoolId, keyword);
        return Ok(new
        {
            count = result.Count,
            data = result
        });
    }
    // 🟢 GET: Lấy danh sách tài khoản theo vai trò
    [HttpGet("staff")]
    public async Task<IActionResult> GetAllStaff(Guid schoolId)
    {
        var result = await _accountService.GetAllAsync(schoolId);
        return Ok(new
        {
            count = result.Count,
            data = result
        });
    }

    /// filletr by role
    [HttpGet("filter-by-role")]
    public async Task<IActionResult> FilterByRole(Guid schoolId, [FromQuery] string role)
    {
        if (string.IsNullOrWhiteSpace(role))
            return BadRequest(new { message = "Role không được để trống." });

        var result = await _accountService.FilterByRoleAsync(schoolId, role);
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
            var account = await _accountService.CreateAsync(request);
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
        var updated = await _accountService.UpdateAsync(userId, request);
        if (updated == null)
            return NotFound(new { message = "Không tìm thấy tài khoản để cập nhật." });

        return Ok(new
        {
            message = "Cập nhật tài khoản thành công!",
            data = updated
        });
    }

    // 🔵 PATCH: Đổi trạng thái kích hoạt
    //[HttpPatch("{userId:guid}/status")]
    //public async Task<IActionResult> ChangeStatus(Guid userId, [FromQuery] bool isActive)
    //{
    //    var result = await _accountService.ChangeStatusAsync(userId, isActive);
    //    if (!result)
    //        return NotFound(new { message = "Không tìm thấy tài khoản." });

    //    return Ok(new { message = $"Đã {(isActive ? "kích hoạt" : "vô hiệu hóa")} tài khoản." });
    //}

    // 🔴 DELETE: Xóa tài khoản
    [HttpDelete("{userId:guid}")]
    public async Task<IActionResult> DeleteAccount(Guid userId)
    {
        var deleted = await _accountService.DeleteAsync(userId);
        if (!deleted)
            return NotFound(new { message = "Không tìm thấy tài khoản để xóa." });

        return Ok(new { message = "Đã xóa tài khoản thành công." });
    }
}
