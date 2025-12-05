using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SMMS.Application.Features.billing.DTOs;
public sealed class CreatePayOsPaymentLinkForInvoiceResult
{
    public long PaymentId { get; init; }
    public string CheckoutUrl { get; init; } = string.Empty;
    public string QrCode { get; init; } = string.Empty;
    public string PaymentLinkId { get; init; } = string.Empty;
}
