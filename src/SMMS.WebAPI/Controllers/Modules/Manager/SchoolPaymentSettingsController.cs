using System.Security.Claims;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SMMS.Application.Features.billing.Commands;
using SMMS.WebAPI.DTOs;

namespace SMMS.WebAPI.Controllers.Modules.Manager;

[ApiController]
[Route("api/v1/schools/payment-settings")]
[Authorize(Roles = "Manager,manager")]
public class SchoolPaymentSettingsController : ControllerBase
{
    private readonly IMediator _mediator;

    public SchoolPaymentSettingsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost("payos/connect")]
    public async Task<IActionResult> ConnectPayOs(
        [FromBody] ConnectPayOsRequest request,
        CancellationToken cancellationToken)
    {
        var schoolId = GetSchoolIdFromToken();

        var command = new ConnectPayOsForSchoolCommand(
            schoolId,
            GetCurrentUserId(),
            request.ClientId,
            request.ApiKey,
            request.ChecksumKey);

        try
        {
            await _mediator.Send(command, cancellationToken);

            return Ok(new
            {
                message = "Kết nối PayOS và cấu hình webhook thành công."
            });
        }
        catch (InvalidOperationException ex)
        {
            // Lỗi do key sai / webhook fail → trả 400 cho trường tự sửa
            return BadRequest(new { error = ex.Message });
        }
    }

    private Guid GetSchoolIdFromToken()
    {
        var schoolIdClaim = User.FindFirst("SchoolId")?.Value;
        if (string.IsNullOrEmpty(schoolIdClaim))
            throw new UnauthorizedAccessException("Không tìm thấy SchoolId trong token.");

        return Guid.Parse(schoolIdClaim);
    }

    private Guid GetCurrentUserId()
    {
        var userIdString = User.FindFirst("UserId")?.Value
                           ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                           ?? User.FindFirst("sub")?.Value
                           ?? User.FindFirst("id")?.Value
                           ?? throw new Exception("Token does not contain UserId.");

        return Guid.Parse(userIdString);
    }
}
