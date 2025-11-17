using MediatR;
using Microsoft.AspNetCore.Mvc;
using SMMS.Application.Features.Manager.Commands;
using SMMS.Application.Features.Manager.DTOs;
using SMMS.Application.Features.Manager.Queries;

namespace SMMS.WebAPI.Controllers.Modules.Manager;

[Route("api/[controller]")]
[ApiController]
public class ManagerPaymentSettingController : ControllerBase
{
    private readonly IMediator _mediator;

    public ManagerPaymentSettingController(IMediator mediator)
    {
        _mediator = mediator;
    }

    // GET: api/ManagerPaymentSetting/school/{schoolId}
    [HttpGet("school/{schoolId:guid}")]
    public async Task<IActionResult> GetBySchool(Guid schoolId)
    {
        var result = await _mediator.Send(new GetSchoolPaymentSettingsQuery(schoolId));

        return Ok(new
        {
            success = true,
            message = "Lấy danh sách cấu hình của trường thành công.",
            data = result
        });
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

    // PUT: api/ManagerPaymentSetting/{settingId}
    [HttpPut("{settingId:int}")]
    public async Task<IActionResult> Update(
        int settingId,
        [FromBody] UpdateSchoolPaymentSettingRequest request)
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

        // Có message nên trả 200 thay vì 204
        return Ok(new
        {
            success = true,
            message = "Xoá cấu hình thành công."
        });
    }
}
