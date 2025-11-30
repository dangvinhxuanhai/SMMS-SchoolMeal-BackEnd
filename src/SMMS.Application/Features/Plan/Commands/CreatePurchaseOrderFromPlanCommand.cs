using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediatR;
using SMMS.Application.Features.Plan.DTOs;

namespace SMMS.Application.Features.Plan.Commands;
// POST: create PO từ Plan
// record reference type + settable props để ModelBinding bind JSON body
public sealed record CreatePurchaseOrderFromPlanCommand
    : IRequest<KsPurchaseOrderDto>
{
    public int PlanId { get; set; }
    public string SupplierName { get; set; } = default!;
    public string? Note { get; set; }
    // User hiện tại (staff) – sau này bạn có thể lấy từ Claims thay vì để FE gửi
    public Guid StaffUserId { get; set; }
    public List<CreatePurchaseOrderLineFromPlanDto> Lines { get; set; } = new();
}

// PUT: update header
public record UpdatePurchaseOrderHeaderCommand(
    int OrderId,
    Guid SchoolId,
    string? SupplierName,
    string? Note,
    DateTime? OrderDate,
    string? Status
) : IRequest<KsPurchaseOrderDetailDto>;

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
) : IRequest<List<KsPurchaseOrderLineDto>>;

// DELETE 1 line
public record DeletePurchaseOrderLineCommand(
    int OrderId,
    int LinesId,
    Guid SchoolId
) : IRequest<Unit>;
