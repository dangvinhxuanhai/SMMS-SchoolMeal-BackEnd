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
public sealed class GetPurchasePlanByDateHandler
    : IRequestHandler<GetPurchasePlanByDateQuery, PurchasePlanDto?>
{
    private readonly IPurchasePlanRepository _repository;

    public GetPurchasePlanByDateHandler(IPurchasePlanRepository repository)
    {
        _repository = repository;
    }

    public Task<PurchasePlanDto?> Handle(
        GetPurchasePlanByDateQuery request,
        CancellationToken cancellationToken)
    {
        return _repository.GetPlanByDateAsync(
            request.SchoolId,
            request.Date,
            cancellationToken);
    }
}
