using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediatR;
using SMMS.Application.Abstractions;
using SMMS.Application.Features.billing.Commands;
using SMMS.Application.Features.billing.Helpers;
using SMMS.Application.Features.billing.Interfaces;

namespace SMMS.Application.Features.billing.Handlers;
public sealed class HandlePayOsWebhookCommandHandler
    : IRequestHandler<HandlePayOsWebhookCommand>
{
    private readonly IPaymentRepository _paymentRepository;
    private readonly IInvoiceRepository _invoiceRepository;
    private readonly ISchoolPaymentGatewayRepository _gatewayRepository;
    private readonly IUnitOfWork _unitOfWork;

    public HandlePayOsWebhookCommandHandler(
        IPaymentRepository paymentRepository,
        IInvoiceRepository invoiceRepository,
        ISchoolPaymentGatewayRepository gatewayRepository,
        IUnitOfWork unitOfWork)
    {
        _paymentRepository = paymentRepository;
        _invoiceRepository = invoiceRepository;
        _gatewayRepository = gatewayRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(
        HandlePayOsWebhookCommand request,
        CancellationToken cancellationToken)
    {
        var payload = request.Payload;

        // 1. Chỉ xử lý khi success + code="00"
        if (!payload.Success || !string.Equals(payload.Code, "00", StringComparison.OrdinalIgnoreCase))
        {
            // Có thể log event failed / canceled ở đây
            return;
        }

        var data = payload.Data;

        // 2. Lấy orderCode = PaymentId
        if (!data.TryGetProperty("orderCode", out var orderCodeElement))
        {
            throw new InvalidOperationException("Webhook data missing orderCode.");
        }

        if (!orderCodeElement.TryGetInt64(out long paymentId))
        {
            throw new InvalidOperationException("orderCode must be an integer (PaymentId).");
        }

        if (!data.TryGetProperty("amount", out var amountElement))
        {
            throw new InvalidOperationException("Webhook data missing amount.");
        }

        if (!amountElement.TryGetInt32(out int amountInt))
        {
            throw new InvalidOperationException("amount must be an integer.");
        }

        decimal paidAmount = amountInt; // VND, không có lẻ

        // Một số field khác (optional)
        string description = data.TryGetProperty("description", out var descEl)
            ? descEl.GetString() ?? string.Empty
            : string.Empty;

        string reference = data.TryGetProperty("reference", out var refEl)
            ? refEl.GetString() ?? string.Empty
            : string.Empty;

        string transactionDateTimeRaw = data.TryGetProperty("transactionDateTime", out var timeEl)
            ? timeEl.GetString() ?? string.Empty
            : string.Empty;

        DateTime paidAt;
        if (!string.IsNullOrWhiteSpace(transactionDateTimeRaw) &&
            DateTime.TryParseExact(transactionDateTimeRaw,
                "yyyy-MM-dd HH:mm:ss",
                CultureInfo.InvariantCulture,
                DateTimeStyles.AssumeLocal,
                out var parsed))
        {
            paidAt = parsed;
        }
        else
        {
            paidAt = DateTime.UtcNow;
        }

        // 3. Tìm Payment
        var payment = await _paymentRepository.GetByIdAsync(paymentId, cancellationToken);
        if (payment is null)
        {
            if (paymentId == 123 ||
                string.Equals(description, "Webhook confirm", StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            throw new InvalidOperationException($"Payment #{paymentId} not found.");
        }

        if (string.Equals(payment.PaymentStatus, "paid", StringComparison.OrdinalIgnoreCase))
        {
            // Đã xử lý rồi → bỏ qua để idempotent
            return;
        }

        // 4. Tìm Invoice
        var invoice = await _invoiceRepository.GetByIdAsync(payment.InvoiceId, cancellationToken);
        if (invoice is null)
        {
            throw new InvalidOperationException($"Invoice #{payment.InvoiceId} not found.");
        }

        // 5. Lấy gateway PayOS đúng trường qua StudentId của Invoice
        var gateway = await _gatewayRepository.GetPayOsGatewayByStudentIdAsync(
            invoice.StudentId,
            cancellationToken);

        if (gateway is null)
        {
            throw new InvalidOperationException("Không tìm thấy PayOS gateway cho học sinh / trường này.");
        }

        // 6. Verify signature
        bool isValidSignature = PayOsSignatureHelper.Verify(
            payload.Data,
            payload.Signature,
            gateway.ChecksumKey);

        if (!isValidSignature)
        {
            throw new InvalidOperationException("Invalid PayOS webhook signature.");
        }

        // (Optional) 7. Kiểm tra amount có khớp không
        if (payment.ExpectedAmount > 0 && payment.ExpectedAmount != paidAmount)
        {
            // Tùy nghiệp vụ: có thể throw hoặc chỉ log warning
            // Ở đây mình cho throw để tránh nhận sai tiền
            throw new InvalidOperationException(
                $"Amount mismatch. Expected {payment.ExpectedAmount}, actual {paidAmount}.");
        }

        // 8. Update Payment
        payment.PaidAmount = paidAmount;
        payment.PaymentStatus = "paid";
        payment.Method = "Bank"; // PayOS là chuyển khoản ngân hàng
        payment.ReferenceNo = reference;
        payment.PaymentContent = string.IsNullOrWhiteSpace(description)
            ? payment.PaymentContent
            : description;
        payment.PaidAt = paidAt;

        // 9. Update Invoice
        invoice.Status = "Paid";

        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
