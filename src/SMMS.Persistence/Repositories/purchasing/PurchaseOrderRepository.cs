using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SMMS.Application.Features.Plan.DTOs;
using SMMS.Application.Features.Plan.Interfaces;
using SMMS.Domain.Entities.purchasing;
using SMMS.Persistence.Data;

namespace SMMS.Persistence.Repositories.purchasing;
public class PurchaseOrderRepository : IPurchaseOrderRepository
{
    private readonly EduMealContext _context;

    public PurchaseOrderRepository(EduMealContext context)
    {
        _context = context;
    }

    public async Task<PurchaseOrder> CreateFromPlanAsync(
        int planId,
        Guid schoolId,
        Guid staffId,
        string? supplierName,
        string? note,
        DateTime? orderDate,
        string? status,
        CancellationToken cancellationToken)
    {
        var plan = await _context.PurchasePlans
            .Include(p => p.PurchasePlanLines)
            .FirstOrDefaultAsync(p => p.PlanId == planId, cancellationToken);

        if (plan == null)
            throw new Exception("Purchase plan not found.");

        if (plan.PurchasePlanLines == null || !plan.PurchasePlanLines.Any())
            throw new Exception("Purchase plan has no lines.");

        var order = new PurchaseOrder
        {
            SchoolId = schoolId,
            OrderDate = orderDate ?? DateTime.UtcNow,
            PurchaseOrderStatus = status ?? "Draft",
            SupplierName = supplierName,
            Note = note,
            PlanId = plan.PlanId,
            StaffInCharged = staffId
        };

        await _context.PurchaseOrders.AddAsync(order, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken); // lấy OrderId

        var lines = plan.PurchasePlanLines.Select(pl => new PurchaseOrderLine
        {
            OrderId = order.OrderId,
            IngredientId = pl.IngredientId,
            QuantityGram = pl.RqQuanityGram,
            UserId = staffId
        }).ToList();

        await _context.PurchaseOrderLines.AddRangeAsync(lines, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        order.PurchaseOrderLines = lines;
        return order;
    }

    public async Task<PurchaseOrder?> GetByIdAsync(
        int orderId,
        Guid schoolId,
        CancellationToken cancellationToken)
    {
        return await _context.PurchaseOrders
            .Include(o => o.PurchaseOrderLines)
                .ThenInclude(l => l.Ingredient)
            .FirstOrDefaultAsync(
                o => o.OrderId == orderId && o.SchoolId == schoolId,
                cancellationToken);
    }

    public async Task<List<PurchaseOrder>> GetListAsync(
        Guid schoolId,
        DateTime? fromDate,
        DateTime? toDate,
        CancellationToken cancellationToken)
    {
        var query = _context.PurchaseOrders
            .Include(o => o.PurchaseOrderLines)
            .Where(o => o.SchoolId == schoolId);

        if (fromDate.HasValue)
            query = query.Where(o => o.OrderDate >= fromDate.Value);

        if (toDate.HasValue)
            query = query.Where(o => o.OrderDate < toDate.Value);

        return await query
            .OrderByDescending(o => o.OrderDate)
            .ToListAsync(cancellationToken);
    }

    public async Task UpdateOrderHeaderAsync(
        int orderId,
        Guid schoolId,
        string? supplierName,
        string? note,
        DateTime? orderDate,
        string? status,
        CancellationToken cancellationToken)
    {
        var order = await _context.PurchaseOrders
            .FirstOrDefaultAsync(o => o.OrderId == orderId && o.SchoolId == schoolId, cancellationToken);

        if (order == null)
            throw new Exception("Purchase order not found.");

        order.SupplierName = supplierName ?? order.SupplierName;
        order.Note = note ?? order.Note;
        if (orderDate.HasValue) order.OrderDate = orderDate.Value;
        if (!string.IsNullOrWhiteSpace(status)) order.PurchaseOrderStatus = status;

        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteOrderAsync(
        int orderId,
        Guid schoolId,
        CancellationToken cancellationToken)
    {
        var order = await _context.PurchaseOrders
            .Include(o => o.PurchaseOrderLines)
            .FirstOrDefaultAsync(o => o.OrderId == orderId && o.SchoolId == schoolId, cancellationToken);

        if (order == null) return;

        _context.PurchaseOrderLines.RemoveRange(order.PurchaseOrderLines);
        _context.PurchaseOrders.Remove(order);

        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task<List<PurchaseOrderLine>> GetOrderLinesAsync(
        int orderId,
        Guid schoolId,
        CancellationToken cancellationToken)
    {
        return await _context.PurchaseOrderLines
            .Include(l => l.Order)
            .Include(l => l.Ingredient)
            .Where(l => l.OrderId == orderId && l.Order.SchoolId == schoolId)
            .ToListAsync(cancellationToken);
    }

    public async Task UpdateOrderLinesAsync(
        int orderId,
        Guid schoolId,
        IEnumerable<PurchaseOrderLineUpdateDto> lines,
        Guid userId,
        CancellationToken cancellationToken)
    {
        var dbLines = await _context.PurchaseOrderLines
            .Include(l => l.Order)
            .Where(l => l.OrderId == orderId && l.Order.SchoolId == schoolId)
            .ToListAsync(cancellationToken);

        foreach (var dto in lines)
        {
            var line = dbLines.FirstOrDefault(l => l.LinesId == dto.LinesId);
            if (line == null) continue;

            line.QuantityGram = dto.QuantityGram;
            line.UnitPrice = dto.UnitPrice;
            line.BatchNo = dto.BatchNo;
            line.Origin = dto.Origin;
            // Fix: Convert DateTime? to DateOnly? for ExpiryDate assignment
            line.ExpiryDate = dto.ExpiryDate.HasValue ? DateOnly.FromDateTime(dto.ExpiryDate.Value) : (DateOnly?)null;
            line.UserId = userId;
        }

        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteOrderLineAsync(
        int linesId,
        int orderId,
        Guid schoolId,
        CancellationToken cancellationToken)
    {
        var line = await _context.PurchaseOrderLines
            .Include(l => l.Order)
            .FirstOrDefaultAsync(
                l => l.LinesId == linesId &&
                     l.OrderId == orderId &&
                     l.Order.SchoolId == schoolId,
                cancellationToken);

        if (line == null) return;

        _context.PurchaseOrderLines.Remove(line);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task AddAsync(PurchaseOrder order, CancellationToken ct = default)
    {
        await _context.PurchaseOrders.AddAsync(order, ct);
    }

    public Task<bool> ExistsForPlanAsync(int planId, CancellationToken ct = default)
    {
        return _context.PurchaseOrders
            .AnyAsync(o => o.PlanId == planId, ct);
    }

    public Task<PurchaseOrder?> GetByIdAsync(int orderId, CancellationToken ct = default)
    {
        return _context.PurchaseOrders
            .Include(o => o.PurchaseOrderLines)
            .FirstOrDefaultAsync(o => o.OrderId == orderId, ct);
    }
}
