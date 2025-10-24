using SMMS.Domain.Entities.billing;
using SMMS.Domain.Entities.purchasing;

namespace SMMS.Application.Features.Manager.Interfaces;

public interface IManagerRepository
{
    IQueryable<Invoice> Invoices { get; }
    IQueryable<Payment> Payments { get; }
    IQueryable<PurchaseOrder> PurchaseOrders { get; }
    IQueryable<PurchaseOrderLine> PurchaseOrderLines { get; }

    Task<int> GetTeacherCountAsync(Guid schoolId);
    Task<int> GetStudentCountAsync(Guid schoolId);
    Task<int> GetClassCountAsync(Guid schoolId);
}
