using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SMMS.Application.Common.Interfaces;
public interface IPayOsIntegrationService
{
    /// <summary>
    /// Gọi API /confirm-webhook của PayOS để đăng ký / cập nhật webhook URL.
    /// Ném InvalidOperationException nếu thất bại.
    /// </summary>
    Task ConfirmWebhookAsync(
        string clientId,
        string apiKey,
        string webhookUrl,
        CancellationToken cancellationToken);
}
