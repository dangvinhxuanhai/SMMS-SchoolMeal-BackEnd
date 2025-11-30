using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SMMS.Application.Features.Plan.DTOs;
using SMMS.Domain.Entities.purchasing;

namespace SMMS.Application.Features.Plan.Interfaces;
public interface IPurchaseOrderRepository
{
    Task<PurchaseOrder> CreateFromPlanAsync(
        int planId,
        Guid schoolId,
        Guid staffId,
        string? supplierName,
        string? note,
        DateTime? orderDate,
        string? status,
        CancellationToken cancellationToken);

    Task<PurchaseOrder?> GetByIdAsync(
        int orderId,
        Guid schoolId,
        CancellationToken cancellationToken);

    Task<List<PurchaseOrder>> GetListAsync(
        Guid schoolId,
        DateTime? fromDate,
        DateTime? toDate,
        CancellationToken cancellationToken);

    Task UpdateOrderHeaderAsync(
        int orderId,
        Guid schoolId,
        string? supplierName,
        string? note,
        DateTime? orderDate,
        string? status,
        CancellationToken cancellationToken);

    Task DeleteOrderAsync(
        int orderId,
        Guid schoolId,
        CancellationToken cancellationToken);

    Task<List<PurchaseOrderLine>> GetOrderLinesAsync(
        int orderId,
        Guid schoolId,
        CancellationToken cancellationToken);

    Task UpdateOrderLinesAsync(
        int orderId,
        Guid schoolId,
        IEnumerable<PurchaseOrderLineUpdateDto> lines,
        Guid userId,
        CancellationToken cancellationToken);

    Task DeleteOrderLineAsync(
        int linesId,
        int orderId,
        Guid schoolId,
        CancellationToken cancellationToken);
    Task AddAsync(PurchaseOrder order, CancellationToken ct = default);

    // Kiểm tra đã có Order nào cho Plan này chưa
    Task<bool> ExistsForPlanAsync(int planId, CancellationToken ct = default);

    Task<PurchaseOrder?> GetByIdAsync(int orderId, CancellationToken ct = default);
}
