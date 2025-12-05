using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediatR;
using Microsoft.Extensions.Options;
using SMMS.Application.Abstractions;
using SMMS.Application.Common.Options;
using SMMS.Application.Features.billing.Commands;
using SMMS.Application.Features.billing.DTOs;
using SMMS.Application.Features.billing.Interfaces;
using SMMS.Domain.Entities.billing;
using PayOS;
using PayOS.Models.V2.PaymentRequests;

namespace SMMS.Application.Features.billing.Handlers;
public sealed class CreatePayOsPaymentLinkForInvoiceCommandHandler
    : IRequestHandler<CreatePayOsPaymentLinkForInvoiceCommand, CreatePayOsPaymentLinkForInvoiceResult>
{
    private readonly IInvoiceRepository _invoiceRepository;
    private readonly IPaymentRepository _paymentRepository;
    private readonly ISchoolPaymentGatewayRepository _gatewayRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly PayOsOptions _payOsOptions;

    public CreatePayOsPaymentLinkForInvoiceCommandHandler(
        IInvoiceRepository invoiceRepository,
        IPaymentRepository paymentRepository,
        ISchoolPaymentGatewayRepository gatewayRepository,
        IUnitOfWork unitOfWork,
        IOptions<PayOsOptions> payOsOptions)
    {
        _invoiceRepository = invoiceRepository;
        _paymentRepository = paymentRepository;
        _gatewayRepository = gatewayRepository;
        _unitOfWork = unitOfWork;
        _payOsOptions = payOsOptions.Value;
    }

    public async Task<CreatePayOsPaymentLinkForInvoiceResult> Handle(
        CreatePayOsPaymentLinkForInvoiceCommand request,
        CancellationToken cancellationToken)
    {
        if (request.Amount <= 0)
            throw new InvalidOperationException("Amount must be greater than 0.");

        // 1. Lấy Invoice
        var invoice = await _invoiceRepository.GetByIdAsync(request.InvoiceId, cancellationToken);
        if (invoice is null)
            throw new InvalidOperationException($"Invoice #{request.InvoiceId} not found.");

        // 2. Lấy gateway PayOS theo StudentId → SchoolId (repo đã join)
        var gateway = await _gatewayRepository.GetPayOsGatewayByStudentIdAsync(
            invoice.StudentId,
            cancellationToken);

        if (gateway is null)
            throw new InvalidOperationException("Trường này chưa cấu hình cổng PayOS.");

        // 3. Tạo Payment 'pending' trước, PaidAmount=0
        var now = DateTime.UtcNow;
        var payment = new Payment
        {
            InvoiceId = invoice.InvoiceId,
            ExpectedAmount = request.Amount,
            PaidAmount = 0m,
            PaymentStatus = "pending",
            PaymentContent = string.IsNullOrWhiteSpace(request.Description)
                ? $"Thanh toán hóa đơn #{invoice.InvoiceId}"
                : request.Description,
            PaidAt = now,
            Method = "Bank",
            ReferenceNo = null
        };

        await _paymentRepository.AddAsync(payment, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken); // để có PaymentId

        long orderCode = payment.PaymentId; // dùng làm orderCode bên PayOS

        // 4. Build returnUrl / cancelUrl
        var returnUrl = _payOsOptions.ReturnUrlTemplate
            .Replace("{invoiceId}", invoice.InvoiceId.ToString(CultureInfo.InvariantCulture));

        var cancelUrl = _payOsOptions.CancelUrlTemplate
            .Replace("{invoiceId}", invoice.InvoiceId.ToString(CultureInfo.InvariantCulture));

        // 5. Gọi PayOS SDK tạo payment link
        var payOS = new PayOSClient(
            gateway.ClientId,
            gateway.ApiKey,
            gateway.ChecksumKey);

        var amountInt = checked((int)request.Amount); // VND nguyên

        var paymentRequest = new CreatePaymentLinkRequest
        {
            OrderCode = orderCode,
            Amount = amountInt,
            Description = payment.PaymentContent,
            CancelUrl = cancelUrl,
            ReturnUrl = returnUrl
        };

        var paymentLink = await payOS.PaymentRequests.CreateAsync(paymentRequest);

        // 6. Cập nhật lại Payment với PaymentLinkId
        payment.ReferenceNo = paymentLink.PaymentLinkId;
        // Có thể update PaymentContent = paymentLink.Description nếu muốn
        payment.PaymentContent = paymentLink.Description ?? payment.PaymentContent;

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // 7. Trả result cho FE
        return new CreatePayOsPaymentLinkForInvoiceResult
        {
            PaymentId = payment.PaymentId,
            CheckoutUrl = paymentLink.CheckoutUrl,
            QrCode = paymentLink.QrCode,
            PaymentLinkId = paymentLink.PaymentLinkId
        };
    }
}
