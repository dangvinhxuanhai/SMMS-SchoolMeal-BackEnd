using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediatR;
using SMMS.Application.Features.Plan.DTOs;

namespace SMMS.Application.Features.Plan.Commands;
// POST: create PO từ Plan
public record CreatePurchaseOrderFromPlanCommand(
    int PlanId,
    Guid SchoolId,
    Guid StaffId,
    string? SupplierName,
    string? Note,
    DateTime? OrderDate,
    string? Status
) : IRequest<PurchaseOrderDetailDto>;

// PUT: update header
public record UpdatePurchaseOrderHeaderCommand(
    int OrderId,
    Guid SchoolId,
    string? SupplierName,
    string? Note,
    DateTime? OrderDate,
    string? Status
) : IRequest<PurchaseOrderDetailDto>;

// DELETE: order
public record DeletePurchaseOrderCommand(
    int OrderId,
    Guid SchoolId
) : IRequest<Unit>;

// PUT lines
public record UpdatePurchaseOrderLinesCommand(
    int OrderId,
    Guid SchoolId,
    Guid UserId,
    List<PurchaseOrderLineUpdateDto> Lines
) : IRequest<List<PurchaseOrderLineDto>>;

// DELETE 1 line
public record DeletePurchaseOrderLineCommand(
    int OrderId,
    int LinesId,
    Guid SchoolId
) : IRequest<Unit>;
