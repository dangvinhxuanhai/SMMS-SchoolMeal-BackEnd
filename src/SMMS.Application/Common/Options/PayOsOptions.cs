using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SMMS.Application.Common.Options;
public sealed class PayOsOptions
{
    public const string SectionName = "PayOS";

    /// <summary>
    /// Base URL của PayOS Merchant API.
    /// </summary>
    public string BaseUrl { get; set; } = "https://api-merchant.payos.vn";

    /// <summary>
    /// Webhook URL của hệ thống vd:
    /// https://api.edumeal.vn/api/v1/webhooks/payos
    /// </summary>
    public string WebhookUrl { get; set; } = string.Empty;

    // URL để PayOS redirect sau khi thanh toán thành công
    public string ReturnUrlTemplate { get; set; } = string.Empty;
    // URL khi người dùng cancel
    public string CancelUrlTemplate { get; set; } = string.Empty;
}
