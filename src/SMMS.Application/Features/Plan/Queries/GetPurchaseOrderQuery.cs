using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediatR;
using SMMS.Application.Features.Plan.DTOs;

namespace SMMS.Application.Features.Plan.Queries;
// GET: detail
public record GetPurchaseOrderByIdQuery(
    int OrderId,
    Guid SchoolId
) : IRequest<KsPurchaseOrderDetailDto?>;

// GET: list by school, optional date range
public record GetPurchaseOrdersBySchoolQuery(
    Guid SchoolId,
    DateTime? FromDate,
    DateTime? ToDate
) : IRequest<List<PurchaseOrderSummaryDto>>;
