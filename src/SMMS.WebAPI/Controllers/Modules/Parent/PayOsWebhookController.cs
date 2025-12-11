using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SMMS.Application.Features.billing.Commands;
using SMMS.Application.Features.billing.DTOs;

namespace SMMS.WebAPI.Controllers.Modules.Parent;
[ApiController]
[Route("api/v1/webhooks/payos")]
[AllowAnonymous]
public class PayOsWebhookController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<PayOsWebhookController> _logger;

    public PayOsWebhookController(
        IMediator mediator,
        ILogger<PayOsWebhookController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    [HttpPost]
    public async Task<IActionResult> Receive(
        [FromBody] PayOsWebhookPayload payload,
        CancellationToken cancellationToken)
    {
        try
        {
            await _mediator.Send(new HandlePayOsWebhookCommand(payload), cancellationToken);
            return Ok(new { message = "Webhook processed" });
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "PayOS webhook invalid: {Message}", ex.Message);
            return BadRequest(new { error = ex.Message });
        }
        catch (System.Exception ex)
        {
            _logger.LogError(ex, "Error while processing PayOS webhook");
            // Có thể vẫn trả 200 để PayOS không retry quá nhiều
            return StatusCode(500, new { error = "Internal error" });
        }
    }
}
