using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediatR;
using SMMS.Application.Features.Plan.DTOs;
using SMMS.Application.Features.Plan.Interfaces;
using SMMS.Application.Features.Plan.Queries;

namespace SMMS.Application.Features.Plan.Handlers;
public sealed class GetPurchasePlanDetailHandler
        : IRequestHandler<GetPurchasePlanDetailQuery, PurchasePlanDto?>
{
    private readonly IPurchasePlanRepository _repository;

    public GetPurchasePlanDetailHandler(IPurchasePlanRepository repository)
    {
        _repository = repository;
    }

    public Task<PurchasePlanDto?> Handle(
        GetPurchasePlanDetailQuery request,
        CancellationToken cancellationToken)
    {
        return _repository.GetPlanDetailAsync(request.PlanId, cancellationToken);
    }
}
