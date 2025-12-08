using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using SMMS.Application.Abstractions;
using SMMS.Application.Features.billing.Commands;
using SMMS.Application.Features.billing.Helpers;
using SMMS.Application.Features.billing.Interfaces;

namespace SMMS.Application.Features.billing.Handlers;

public sealed class HandlePayOsWebhookCommandHandler : IRequestHandler<HandlePayOsWebhookCommand>
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

    public async Task Handle(HandlePayOsWebhookCommand request, CancellationToken cancellationToken)
    {
        var payload = request.Payload;
        var data = payload.Data;

        // 1. Nếu giao dịch thất bại hoặc lỗi -> Bỏ qua
        if (!payload.Success || !string.Equals(payload.Code, "00", StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

        // 2. Lấy OrderCode (chính là PaymentId trong hệ thống của bạn)
        if (!data.TryGetProperty("orderCode", out var orderCodeElement) ||
            !orderCodeElement.TryGetInt64(out long paymentId))
        {
            return; // Không có orderCode thì không xử lý được
        }

        // Lấy số tiền thực trả
        int paidAmountInt = 0;
        if (data.TryGetProperty("amount", out var amountElement))
        {
            amountElement.TryGetInt32(out paidAmountInt);
        }
        decimal paidAmount = paidAmountInt;

        // 3. Tìm Payment trong Database
        var payment = await _paymentRepository.GetByIdAsync(paymentId, cancellationToken);

        // --- XỬ LÝ TEST WEBHOOK CỦA PAYOS ---
        if (payment is null)
        {
            string description = data.TryGetProperty("description", out var d) ? d.GetString() : "";
            if (paymentId == 123 || string.Equals(description, "Webhook confirm", StringComparison.OrdinalIgnoreCase))
            {
                return; // Trả về thành công để PayOS biết kết nối OK
            }
            throw new InvalidOperationException($"Payment #{paymentId} not found.");
        }

        // --- IDEMPOTENCY: CHỐNG TRÙNG LẶP ---
        if (string.Equals(payment.PaymentStatus, "paid", StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

        // 4. Tìm Invoice tương ứng
        // Biến 'invoice' được khai báo ở đây để dùng chung
        var invoice = await _invoiceRepository.GetByIdAsync(payment.InvoiceId, cancellationToken);
        if (invoice is null)
        {
            throw new InvalidOperationException($"Invoice #{payment.InvoiceId} not found.");
        }

        // 5. Xác thực chữ ký (Signature)
        var gateway = await _gatewayRepository.GetPayOsGatewayByStudentIdAsync(invoice.StudentId, cancellationToken);
        if (gateway == null) throw new InvalidOperationException("Gateway configuration not found.");

        bool isValid = PayOsSignatureHelper.Verify(payload.Data, payload.Signature, gateway.ChecksumKey);

        if (!isValid)
        {
            throw new InvalidOperationException("Invalid Signature. Checksum Key mismatch.");
        }

        // 6. CẬP NHẬT TRẠNG THÁI (UPDATE DATABASE)
        try
        {
            // A. Cập nhật bảng Payment
            payment.PaymentStatus = "paid";
            payment.PaidAmount = paidAmount;
            payment.PaidAt = DateTime.UtcNow;

            // B. Cập nhật bảng Invoice
            // (SỬA LỖI Ở ĐÂY: Dùng lại biến 'invoice' ở bước 4, không khai báo lại, không check null sai logic)
            invoice.Status = "Paid";

            // C. Lưu thay đổi
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Database Update Failed: {ex.Message}");
        }
    }
}
