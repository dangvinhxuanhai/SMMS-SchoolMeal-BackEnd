using Microsoft.EntityFrameworkCore;
using SMMS.Domain.Entities.billing;
using SMMS.Domain.Entities.purchasing;
using SMMS.Persistence.Data;
using SMMS.Application.Features.Manager.Interfaces;

namespace SMMS.Persistence.Repositories.Manager;

public class ManagerRepository : IManagerRepository
{
    private readonly EduMealContext _context;

    public ManagerRepository(EduMealContext context)
    {
        _context = context;
    }

    // 🔹 Expose IQueryable cho Application layer
    public IQueryable<Invoice> Invoices => _context.Invoices.AsNoTracking();
    public IQueryable<Payment> Payments => _context.Payments.AsNoTracking();
    public IQueryable<PurchaseOrder> PurchaseOrders => _context.PurchaseOrders.AsNoTracking();
    public IQueryable<PurchaseOrderLine> PurchaseOrderLines => _context.PurchaseOrderLines.AsNoTracking();

    // 🔸 Thống kê số lượng theo trường học
    public async Task<int> GetTeacherCountAsync(Guid schoolId)
        => await _context.Teachers
            .Include(t => t.TeacherNavigation) // liên kết sang User
            .AsNoTracking()
            .CountAsync(t => t.TeacherNavigation.SchoolId == schoolId);
    public async Task<int> GetStudentCountAsync(Guid schoolId)
        => await _context.Students
            .AsNoTracking()
            .CountAsync(s => s.SchoolId == schoolId);

    public async Task<int> GetClassCountAsync(Guid schoolId)
        => await _context.Classes
            .AsNoTracking()
            .CountAsync(c => c.SchoolId == schoolId);
}
