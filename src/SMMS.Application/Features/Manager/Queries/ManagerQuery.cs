using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediatR;
using SMMS.Application.Features.Manager.DTOs;

namespace SMMS.Application.Features.Manager.Queries;
public record GetManagerOverviewQuery(Guid SchoolId)
    : IRequest<ManagerOverviewDto>;

public record GetRecentPurchasesQuery(Guid SchoolId, int Take = 8)
    : IRequest<List<RecentPurchaseDto>>;

public record GetPurchaseOrderDetailsQuery(int OrderId)
    : IRequest<List<PurchaseOrderLineDto>>;

public record GetRevenueQuery(
    Guid SchoolId,
    DateTime From,
    DateTime To,
    string Granularity = "daily")
    : IRequest<RevenueSeriesDto>;
