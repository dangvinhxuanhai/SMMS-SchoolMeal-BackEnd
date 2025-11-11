using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SMMS.Application.Features.Manager.Interfaces;
using SMMS.Domain.Entities.billing;
using SMMS.Domain.Entities.purchasing;
using SMMS.Persistence.Data;

namespace SMMS.Persistence.Repositories.Manager;
public class ManagerFinanceRepository : IManagerFinanceRepository
{
    private readonly EduMealContext _context;

    public ManagerFinanceRepository(EduMealContext context)
    {
        _context = context;
    }

    public IQueryable<Invoice> Invoices => _context.Invoices.AsQueryable();
    public IQueryable<Payment> Payments => _context.Payments.AsQueryable();
    public IQueryable<PurchaseOrder> PurchaseOrders => _context.PurchaseOrders.AsQueryable();
    public IQueryable<PurchaseOrderLine> PurchaseOrderLines => _context.PurchaseOrderLines.AsQueryable();

    public async Task<List<Invoice>> GetInvoicesBySchoolAsync(Guid schoolId)
    {
        return await _context.Invoices
            .Include(i => i.Student)
            .ThenInclude(s => s.StudentClasses)
            .ThenInclude(sc => sc.Class)
            .Where(i => i.Student.StudentClasses.Any(sc => sc.Class.SchoolId == schoolId))
            .OrderByDescending(i => i.DateTo)
            .ToListAsync();
    }

    public async Task<Invoice?> GetInvoiceDetailAsync(long invoiceId)
    {
        return await _context.Invoices
            .Include(i => i.Student)
            .ThenInclude(s => s.StudentClasses)
            .ThenInclude(sc => sc.Class)
            .Include(i => i.Payments)
            .FirstOrDefaultAsync(i => i.InvoiceId == invoiceId);
    }

    public async Task<List<PurchaseOrder>> GetPurchaseOrdersByMonthAsync(Guid schoolId, int month, int year)
    {
        return await _context.PurchaseOrders
            .Where(po => po.SchoolId == schoolId &&
                         po.OrderDate.Month == month &&
                         po.OrderDate.Year == year)
            .Include(po => po.PurchaseOrderLines)
            .ToListAsync();
    }

    public async Task<PurchaseOrder?> GetPurchaseOrderDetailAsync(int orderId)
    {
        return await _context.PurchaseOrders
            .Include(po => po.PurchaseOrderLines)
            .FirstOrDefaultAsync(po => po.OrderId == orderId);
    }

}
