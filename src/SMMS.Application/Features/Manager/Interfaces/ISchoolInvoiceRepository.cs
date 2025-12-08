using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SMMS.Application.Features.Manager.DTOs;
using SMMS.Domain.Entities.billing;
using SMMS.Domain.Entities.school;

namespace SMMS.Application.Features.Manager.Interfaces;
public interface ISchoolInvoiceRepository
{
    // IQueryable (cho handler tự xử lý)
    IQueryable<Invoice> Invoices { get; }
    IQueryable<Student> Students { get; }
    IQueryable<Attendance> Attendance { get; }

    void Update(Invoice invoice);

    // Command operations
    Task AddInvoiceAsync(Invoice invoice, CancellationToken ct);
    Task AddInvoicesAsync(IEnumerable<Invoice> invoices, CancellationToken ct);
    Task<bool> SaveChangesAsync(CancellationToken ct);

    Task<bool> DeleteInvoiceAsync(Invoice invoice, CancellationToken ct);
}
