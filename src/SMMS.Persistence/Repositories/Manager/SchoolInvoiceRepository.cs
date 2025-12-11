using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SMMS.Application.Features.Manager.DTOs;
using SMMS.Application.Features.Manager.Interfaces;
using SMMS.Domain.Entities.billing;
using SMMS.Domain.Entities.school;
using SMMS.Persistence.Data;
using Microsoft.EntityFrameworkCore;
namespace SMMS.Persistence.Repositories.Manager;
public class SchoolInvoiceRepository : ISchoolInvoiceRepository
{
    private readonly EduMealContext _context;

    public SchoolInvoiceRepository(EduMealContext context)
    {
        _context = context;
    }

    public IQueryable<Invoice> Invoices => _context.Invoices.AsNoTracking();
    public IQueryable<Student> Students => _context.Students.AsNoTracking();
    public IQueryable<Attendance> Attendance => _context.Attendances.AsNoTracking();

    public async Task AddInvoiceAsync(Invoice invoice, CancellationToken ct)
    {
        await _context.Invoices.AddAsync(invoice, ct);
    }
    public void Update(Invoice invoice)
    {
        // nếu Invoices ở trên là AsNoTracking thì dòng này sẽ Attach + mark Modified
        _context.Invoices.Update(invoice);
        // hoặc: _context.Entry(invoice).State = EntityState.Modified;
    }
    public async Task AddInvoicesAsync(IEnumerable<Invoice> invoices, CancellationToken ct)
    {
        await _context.Invoices.AddRangeAsync(invoices, ct);
    }

    public async Task<bool> SaveChangesAsync(CancellationToken ct)
    {
        return await _context.SaveChangesAsync(ct) > 0;
    }

    public async Task<bool> DeleteInvoiceAsync(Invoice invoice, CancellationToken ct)
    {
        _context.Invoices.Remove(invoice);
        return await _context.SaveChangesAsync(ct) > 0;
    }
}
