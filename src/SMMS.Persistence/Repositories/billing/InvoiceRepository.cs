using Microsoft.EntityFrameworkCore;
using SMMS.Application.Features.billing.DTOs;
using SMMS.Application.Features.billing.Interfaces;
using SMMS.Domain.Entities.billing;
using SMMS.Domain.Entities.school;
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

        //Lấy hóa đơn của con chưa thanh toán
        public async Task<IEnumerable<InvoiceDto>> GetUnpaidInvoicesAsync(Guid studentId)
        {
            var schoolId = await _context.Students
            .Where(s => s.StudentId == studentId)
            .Select(s => s.SchoolId)
            .FirstOrDefaultAsync();

            if (schoolId == Guid.Empty)
                return Enumerable.Empty<InvoiceDto>();

            // 2️⃣ Lấy cấu hình thanh toán
            var setting = await _context.SchoolPaymentSettings
                .Where(s => s.SchoolId == schoolId && s.IsActive)
                .FirstOrDefaultAsync();

            if (setting == null)
                return Enumerable.Empty<InvoiceDto>();

            var query =
                from inv in _context.Invoices
                join stu in _context.Students
                    on inv.StudentId equals stu.StudentId
                where stu.StudentId == studentId
                      && inv.Status == "Unpaid"
                select new InvoiceDto
                {
                    InvoiceId = inv.InvoiceId,
                    InvoiceCode = inv.InvoiceCode,
                    StudentName = stu.FullName,
                    MonthNo = inv.MonthNo,
                    DateFrom = inv.DateFrom.ToDateTime(TimeOnly.MinValue),
                    DateTo = inv.DateTo.ToDateTime(TimeOnly.MinValue),
                    AbsentDay = inv.AbsentDay,
                    Status = inv.Status,
                    AmountToPay = Math.Max(0, setting.TotalAmount - (inv.AbsentDay) * setting.MealPricePerDay)
                };

            return await query.ToListAsync();
        }
        // ✅ Danh sách hóa đơn của các con thuộc phụ huynh
        public async Task<IEnumerable<InvoiceDto>> GetInvoicesByParentAsync(Guid studentId)
        {
            var query = from inv in _context.Invoices
                        join stu in _context.Students on inv.StudentId equals stu.StudentId
                        where stu.StudentId == studentId
                        orderby inv.DateFrom descending
                        select new InvoiceDto
                        {
                            InvoiceId = inv.InvoiceId,
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
        public async Task<InvoiceDetailDto?> GetInvoiceDetailAsync(long invoiceId, Guid studentId)
        {
            var schoolId = await _context.Students
                .Where(s => s.StudentId == studentId)
                .Select(s => s.SchoolId)
                .FirstOrDefaultAsync();
            var setting = await _context.SchoolPaymentSettings
              .Where(s => s.SchoolId == schoolId && s.IsActive)
              .FirstOrDefaultAsync();
            return await (
                from inv in _context.Invoices

                    // Học sinh
                join stu in _context.Students
                    on inv.StudentId equals stu.StudentId

                // Lớp học (lấy lớp hiện tại — bản ghi chưa có LeftDate)
                join scCls in _context.StudentClasses
                    on stu.StudentId equals scCls.StudentId
                join cls in _context.Classes
                    on scCls.ClassId equals cls.ClassId

                // Trường
                join sch in _context.Schools
                    on stu.SchoolId equals sch.SchoolId

                // Payment: LEFT JOIN (Unpaid có thể không có payment)
                join pay in _context.Payments
                    on inv.InvoiceId equals pay.InvoiceId into payGroup
                from payment in payGroup.DefaultIfEmpty()

                where
                    inv.InvoiceId == invoiceId
                    && stu.StudentId == studentId
                    && scCls.LeftDate == null    // chỉ lấy lớp hiện tại

                select new InvoiceDetailDto
                {
                    InvoiceId = inv.InvoiceId,
                    InvoiceCode = inv.InvoiceCode,
                    StudentName = stu.FullName,
                    ClassName = cls.ClassName,
                    SchoolName = sch.SchoolName,

                    MonthNo = inv.MonthNo,
                    DateFrom = inv.DateFrom.ToDateTime(TimeOnly.MinValue),
                    DateTo = inv.DateTo.ToDateTime(TimeOnly.MinValue),
                    AbsentDay = inv.AbsentDay,
                    Status = inv.Status,

                    // Số tiền phải đóng
                    AmountToPay = Math.Max(0, setting.TotalAmount - (inv.AbsentDay) * setting.MealPricePerDay),

                    // 🏦 Thông tin ngân hàng của trường
                    SettlementBankCode = sch.SettlementBankCode ?? string.Empty,
                    SettlementAccountNo = sch.SettlementAccountNo ?? string.Empty,
                    SettlementAccountName = sch.SettlementAccountName ?? string.Empty,
                }
            ).FirstOrDefaultAsync();
        }

        public Task<Invoice?> GetByIdAsync(long invoiceId, CancellationToken ct)
        {
            return _context.Invoices
                .FirstOrDefaultAsync(i => i.InvoiceId == invoiceId, ct);
        }
    }
}
