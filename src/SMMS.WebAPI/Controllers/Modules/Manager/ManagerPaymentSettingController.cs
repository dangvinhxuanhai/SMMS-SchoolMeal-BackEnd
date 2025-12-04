using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SMMS.Application.Features.Manager.Commands;
using SMMS.Application.Features.Manager.DTOs;
using SMMS.Application.Features.Manager.Queries;

namespace SMMS.WebAPI.Controllers.Modules.Manager;

[Route("api/[controller]")]
[ApiController]
[Authorize(Roles = "Manager")]
public class ManagerPaymentSettingController : ControllerBase
{
    private readonly IMediator _mediator;

    public ManagerPaymentSettingController(IMediator mediator)
    {
        _mediator = mediator;
    }

    private Guid GetSchoolIdFromToken()
    {
        // Kiểm tra cả 2 trường hợp viết hoa/thường của Claim Key
        var schoolIdClaim = User.FindFirst("SchoolId")?.Value
                         ?? User.FindFirst("schoolId")?.Value;

        if (string.IsNullOrEmpty(schoolIdClaim))
            throw new UnauthorizedAccessException("Không tìm thấy SchoolId trong token.");

        if (!Guid.TryParse(schoolIdClaim, out var schoolId))
            throw new UnauthorizedAccessException("SchoolId trong token không hợp lệ.");

        return schoolId;
    }

    [HttpGet("school/current")]
    public async Task<IActionResult> GetByCurrentSchool()
    {
        try
        {
            // Backend tự lấy ID từ Token
            var schoolId = GetSchoolIdFromToken();
            var result = await _mediator.Send(new GetSchoolPaymentSettingsQuery(schoolId));

            return Ok(new
            {
                success = true,
                message = "Lấy danh sách cấu hình của trường thành công.",
                data = result
            });
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(new { success = false, message = ex.Message });
        }
    }

    // GET: api/ManagerPaymentSetting/{settingId}
    [HttpGet("{settingId:int}")]
    public async Task<IActionResult> GetById(int settingId)
    {
        var result = await _mediator.Send(new GetSchoolPaymentSettingByIdQuery(settingId));
        if (result == null)
        {
            return NotFound(new
            {
                success = false,
                message = "Không tìm thấy cấu hình với mã này."
            });
        }

        return Ok(new
        {
            success = true,
            message = "Lấy thông tin cấu hình thành công.",
            data = result
        });
    }

    // POST: api/ManagerPaymentSetting
    [HttpPost]
    public async Task<IActionResult> Create(
        [FromBody] CreateSchoolPaymentSettingRequest request)
    {
        try
        {
            var schoolId = GetSchoolIdFromToken();
            request.SchoolId = schoolId;

            var created = await _mediator.Send(new CreateSchoolPaymentSettingCommand(request));

            return CreatedAtAction(nameof(GetById),
                new { settingId = created.SettingId },
                new
                {
                    success = true,
                    message = "Tạo cấu hình mới thành công.",
                    data = created
                });
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(new { success = false, message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { success = false, message = ex.Message });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { success = false, message = ex.Message });
        }
    }

    // PUT: api/ManagerPaymentSetting/{settingId}
    [HttpPut("{settingId:int}")]
    public async Task<IActionResult> Update(
        int settingId,
        [FromBody] UpdateSchoolPaymentSettingRequest request)
    {
        try
        {

            var updated = await _mediator.Send(
                new UpdateSchoolPaymentSettingCommand(settingId, request));

            if (updated == null)
            {
                return NotFound(new
                {
                    success = false,
                    message = "Không tìm thấy cấu hình để cập nhật."
                });
            }

            return Ok(new
            {
                success = true,
                message = "Cập nhật cấu hình thành công.",
                data = updated
            });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { success = false, message = ex.Message });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { success = false, message = ex.Message });
        }
    }

    // DELETE: api/ManagerPaymentSetting/{settingId}
    [HttpDelete("{settingId:int}")]
    public async Task<IActionResult> Delete(int settingId)
    {
        var ok = await _mediator.Send(new DeleteSchoolPaymentSettingCommand(settingId));
        if (!ok)
        {
            return NotFound(new
            {
                success = false,
                message = "Không tìm thấy cấu hình để xoá."
            });
        }

        return Ok(new
        {
            success = true,
            message = "Xoá cấu hình thành công."
        });
    }
}
