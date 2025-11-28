using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediatR;
using SMMS.Application.Features.Plan.Commands;
using SMMS.Application.Features.Plan.Interfaces;

namespace SMMS.Application.Features.Plan.Handlers;
public sealed class SoftDeletePurchasePlanHandler
        : IRequestHandler<SoftDeletePurchasePlanCommand, Unit>
{
    private readonly IPurchasePlanRepository _repository;

    public SoftDeletePurchasePlanHandler(IPurchasePlanRepository repository)
    {
        _repository = repository;
    }

    public async Task<Unit> Handle(
        SoftDeletePurchasePlanCommand request,
        CancellationToken cancellationToken)
    {
        await _repository.SoftDeletePlanAsync(request.PlanId, cancellationToken);
        return Unit.Value;
    }
}
