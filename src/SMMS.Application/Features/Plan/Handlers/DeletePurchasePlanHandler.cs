using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediatR;
using SMMS.Application.Features.Plan.Commands;
using SMMS.Application.Features.Plan.Interfaces;

namespace SMMS.Application.Features.Plan.Handlers;
public sealed class DeletePurchasePlanHandler
        : IRequestHandler<DeletePurchasePlanCommand, Unit>
{
    private readonly IPurchasePlanRepository _repository;

    public DeletePurchasePlanHandler(IPurchasePlanRepository repository)
    {
        _repository = repository;
    }

    public async Task<Unit> Handle(
        DeletePurchasePlanCommand request,
        CancellationToken cancellationToken)
    {
        await _repository.DeletePlanAsync(request.PlanId, cancellationToken);
        return Unit.Value;
    }
}
