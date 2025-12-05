
namespace SMMS.WebAPI.DTOs;
public sealed class CreatePayOsPaymentLinkResponse
{
    public long PaymentId { get; set; }
    public string CheckoutUrl { get; set; } = string.Empty;
    public string QrCode { get; set; } = string.Empty;
    public string PaymentLinkId { get; set; } = string.Empty;
}
