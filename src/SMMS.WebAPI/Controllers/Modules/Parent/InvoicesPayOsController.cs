using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SMMS.Application.Features.billing.Commands;
using SMMS.WebAPI.DTOs;

namespace SMMS.WebAPI.Controllers.Modules.Parent;

[ApiController]
[Route("api/v1/billing/invoices")]
[Authorize(Roles = "Parent,Manager,manager,parent")]
public class InvoicesPayOsController : ControllerBase
{
    private readonly IMediator _mediator;

    public InvoicesPayOsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost("{invoiceId:long}/payos/create-payment-link")]
    public async Task<ActionResult<CreatePayOsPaymentLinkResponse>> CreatePayOsPaymentLink(
        long invoiceId,
        [FromBody] CreatePayOsPaymentLinkRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var command = new CreatePayOsPaymentLinkForInvoiceCommand(
                invoiceId,
                request.Amount,
                request.Description ?? string.Empty);

            var result = await _mediator.Send(command, cancellationToken);

            var response = new CreatePayOsPaymentLinkResponse
            {
                PaymentId = result.PaymentId,
                CheckoutUrl = result.CheckoutUrl,
                QrCode = result.QrCode,
                PaymentLinkId = result.PaymentLinkId
            };

            return Ok(response);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }
}
