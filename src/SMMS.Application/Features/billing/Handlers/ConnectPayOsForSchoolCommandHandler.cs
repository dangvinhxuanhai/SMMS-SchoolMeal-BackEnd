using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediatR;
using Microsoft.Extensions.Options;
using SMMS.Application.Abstractions;
using SMMS.Application.Common.Interfaces;
using SMMS.Application.Common.Options;
using SMMS.Application.Features.billing.Commands;
using SMMS.Application.Features.billing.Interfaces;
using SMMS.Application.Features.Identity.Interfaces;
using SMMS.Domain.Entities.billing;

namespace SMMS.Application.Features.billing.Handlers;
public sealed class ConnectPayOsForSchoolCommandHandler
    : IRequestHandler<ConnectPayOsForSchoolCommand>
{
    private readonly IPayOsIntegrationService _payOsIntegrationService;
    private readonly ISchoolPaymentGatewayRepository _gatewayRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly PayOsOptions _payOsOptions;

    public ConnectPayOsForSchoolCommandHandler(
        IPayOsIntegrationService payOsIntegrationService,
        ISchoolPaymentGatewayRepository gatewayRepository,
        IUnitOfWork unitOfWork,
        IOptions<PayOsOptions> payOsOptions)
    {
        _payOsIntegrationService = payOsIntegrationService;
        _gatewayRepository = gatewayRepository;
        _unitOfWork = unitOfWork;
        _payOsOptions = payOsOptions.Value;
    }

    public async Task Handle(
        ConnectPayOsForSchoolCommand request,
        CancellationToken cancellationToken)
    {
        // 1. Gọi PayOS /confirm-webhook để xác nhận key + set webhook URL
        await _payOsIntegrationService.ConfirmWebhookAsync(
            request.ClientId,
            request.ApiKey,
            _payOsOptions.WebhookUrl,
            cancellationToken);

        // 2. Upsert config trong bảng SchoolPaymentGateways
        var existing = await _gatewayRepository.GetPayOsGatewayAsync(
            request.SchoolId,
            cancellationToken);

        var now = DateTime.UtcNow;

        if (existing is null)
        {
            var gateway = new SchoolPaymentGateway
            {
                SchoolId = request.SchoolId,
                TheProvider = "PayOS",
                ClientId = request.ClientId,
                ApiKey = request.ApiKey,
                ChecksumKey = request.ChecksumKey,
                IsActive = true,
                CreatedAt = now,
                CreatedBy = request.CreatedBy
            };

            await _gatewayRepository.AddAsync(gateway, cancellationToken);
        }
        else
        {
            existing.ClientId = request.ClientId;
            existing.ApiKey = request.ApiKey;
            existing.ChecksumKey = request.ChecksumKey;
            existing.IsActive = true;
            existing.UpdatedAt = now;
            existing.UpdatedBy = request.CreatedBy;
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
