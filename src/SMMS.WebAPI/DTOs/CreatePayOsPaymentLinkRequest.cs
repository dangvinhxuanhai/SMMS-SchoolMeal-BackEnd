
namespace SMMS.WebAPI.DTOs;
public sealed class CreatePayOsPaymentLinkRequest
{
    public decimal Amount { get; set; }
    public string? Description { get; set; }
}
