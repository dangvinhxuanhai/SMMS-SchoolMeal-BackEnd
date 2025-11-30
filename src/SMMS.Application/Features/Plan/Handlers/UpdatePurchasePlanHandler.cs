using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediatR;
using SMMS.Application.Features.Plan.Commands;
using SMMS.Application.Features.Plan.DTOs;
using SMMS.Application.Features.Plan.Interfaces;

namespace SMMS.Application.Features.Plan.Handlers;
public sealed class UpdatePurchasePlanHandler
        : IRequestHandler<UpdatePurchasePlanCommand, PurchasePlanDto>
{
    private readonly IPurchasePlanRepository _repository;

    public UpdatePurchasePlanHandler(IPurchasePlanRepository repository)
    {
        _repository = repository;
    }

    public async Task<PurchasePlanDto> Handle(
        UpdatePurchasePlanCommand request,
        CancellationToken cancellationToken)
    {
        await _repository.UpdatePlanWithLinesAsync(
            request.PlanId,
            request.PlanStatus,
            request.ConfirmedBy,
            request.Lines,
            cancellationToken);

        var dto = await _repository.GetPlanDetailAsync(request.PlanId, cancellationToken);
        if (dto == null)
        {
            throw new InvalidOperationException("Purchase plan not found after update.");
        }

        return dto;
    }
}
