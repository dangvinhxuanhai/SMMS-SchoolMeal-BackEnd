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
public sealed class GetPurchasePlansHandler
    : IRequestHandler<GetPurchasePlansQuery, List<PurchasePlanListItemDto>>
{
    private readonly IPurchasePlanRepository _repository;

    public GetPurchasePlansHandler(IPurchasePlanRepository repository)
    {
        _repository = repository;
    }

    public Task<List<PurchasePlanListItemDto>> Handle(
        GetPurchasePlansQuery request,
        CancellationToken cancellationToken)
    {
        return _repository.GetPlansForSchoolAsync(
            request.SchoolId,
            request.IncludeDeleted,
            cancellationToken);
    }
}
