using Microsoft.EntityFrameworkCore;
using SMMS.Application.Features.billing.DTOs;
using SMMS.Application.Features.billing.Interfaces;
using SMMS.Persistence.Data;

namespace SMMS.Infrastructure.Repositories
{
    public class InvoiceRepository : IInvoiceRepository
    {
        private readonly EduMealContext _context;

        public InvoiceRepository(EduMealContext context)
        {
            _context = context;
        }

        // ✅ Danh sách hóa đơn của các con thuộc phụ huynh
        public async Task<IEnumerable<InvoiceDto>> GetInvoicesByParentAsync(Guid parentId)
        {
            var query = from inv in _context.Invoices
                        join stu in _context.Students on inv.StudentId equals stu.StudentId
                        where stu.ParentId == parentId
                        orderby inv.DateFrom descending
                        select new InvoiceDto
                        {
                            InvoiceId = inv.InvoiceId,
                            StudentId = stu.StudentId,
                            StudentName = stu.FullName,
                            MonthNo = inv.MonthNo,
                            DateFrom = inv.DateFrom.ToDateTime(TimeOnly.MinValue),
                            DateTo = inv.DateTo.ToDateTime(TimeOnly.MinValue),
                            AbsentDay = inv.AbsentDay,
                            Status = inv.Status
                        };

            return await query.ToListAsync();
        }

        // ✅ Chi tiết hóa đơn
        public async Task<InvoiceDto?> GetInvoiceDetailAsync(long invoiceId, Guid parentId)
        {
            var result = await (
                from inv in _context.Invoices
                join stu in _context.Students on inv.StudentId equals stu.StudentId
                where inv.InvoiceId == invoiceId && stu.ParentId == parentId
                select new InvoiceDto
                {
                    InvoiceId = inv.InvoiceId,
                    StudentId = stu.StudentId,
                    StudentName = stu.FullName,
                    MonthNo = inv.MonthNo,
                    DateFrom = inv.DateFrom.ToDateTime(TimeOnly.MinValue),
                    DateTo = inv.DateTo.ToDateTime(TimeOnly.MinValue),
                    AbsentDay = inv.AbsentDay,
                    Status = inv.Status
                }).FirstOrDefaultAsync();

            return result;
        }
    }
}
