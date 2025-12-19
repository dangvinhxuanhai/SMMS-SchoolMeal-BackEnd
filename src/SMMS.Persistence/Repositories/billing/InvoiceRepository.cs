using Microsoft.EntityFrameworkCore;
using PayOS.Models.V2.PaymentRequests.Invoices;
using SMMS.Application.Features.billing.DTOs;
using SMMS.Application.Features.billing.Interfaces;
using SMMS.Domain.Entities.billing;
using SMMS.Domain.Entities.school;
using SMMS.Persistence.Data;
using BillingInvoice = SMMS.Domain.Entities.billing.Invoice;

namespace SMMS.Persistence.Repositories.billing
{
    public class InvoiceRepository : IInvoiceRepository
    {
        private readonly EduMealContext _context;

        public InvoiceRepository(EduMealContext context)
        {
            _context = context;
        }

        // Lấy hóa đơn của con chưa thanh toán
        public async Task<IEnumerable<InvoiceDto>> GetUnpaidInvoicesAsync(Guid studentId)
        {
            var schoolId = await _context.Students
                .Where(s => s.StudentId == studentId)
                .Select(s => s.SchoolId)
                .FirstOrDefaultAsync();

            if (schoolId == Guid.Empty)
                return Enumerable.Empty<InvoiceDto>();

            int holidayCount = await CalculateHolidaysAsync(schoolId);

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
                    Holiday = holidayCount,
                    Status = inv.Status,
                    AmountToPay = inv.TotalPrice
                };

            return await query.ToListAsync();
        }

        public async Task<IEnumerable<InvoiceDto>> GetInvoicesByParentAsync(Guid studentId)
        {
            var query = from inv in _context.Invoices
                join stu in _context.Students on inv.StudentId equals stu.StudentId
                where stu.StudentId == studentId
                orderby inv.DateFrom descending
                select new InvoiceDto
                {
                    InvoiceId = inv.InvoiceId,
                    InvoiceCode = inv.InvoiceCode, // <--- Đã thêm dòng này
                    StudentName = stu.FullName,
                    MonthNo = inv.MonthNo,
                    DateFrom = inv.DateFrom.ToDateTime(TimeOnly.MinValue),
                    DateTo = inv.DateTo.ToDateTime(TimeOnly.MinValue),
                    AbsentDay = inv.AbsentDay,
                    Status = inv.Status,
                    AmountToPay = inv.TotalPrice // Nên thêm cái này để hiển thị số tiền ở danh sách
                };

            return await query.ToListAsync();
        }

        public async Task<InvoiceDetailDto?> GetInvoiceDetailAsync(long invoiceId, Guid studentId)
        {
            var schoolId = await _context.Students
                .Where(s => s.StudentId == studentId)
                .Select(s => s.SchoolId)
                .FirstOrDefaultAsync();

            int holidayCount = await CalculateHolidaysAsync(schoolId);

            var mealPricePerDay = await _context.SchoolPaymentSettings
                .Where(s => s.SchoolId == schoolId && s.IsActive)
                .Select(s => s.MealPricePerDay)
                .FirstOrDefaultAsync();

            return await (
                from inv in _context.Invoices
                join stu in _context.Students
                    on inv.StudentId equals stu.StudentId
                join scCls in _context.StudentClasses
                    on stu.StudentId equals scCls.StudentId
                join cls in _context.Classes
                    on scCls.ClassId equals cls.ClassId
                join sch in _context.Schools
                    on stu.SchoolId equals sch.SchoolId
                join pay in _context.Payments
                    on inv.InvoiceId equals pay.InvoiceId into payGroup
                from payment in payGroup.DefaultIfEmpty()
                where
                    inv.InvoiceId == invoiceId
                    && stu.StudentId == studentId
                    && scCls.LeftDate == null
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
                    Holiday = holidayCount,
                    Status = inv.Status,
                    MealPricePerDay = mealPricePerDay,
                    AmountToPay = inv.TotalPrice,
                    SettlementBankCode = sch.SettlementBankCode ?? string.Empty,
                    SettlementAccountNo = sch.SettlementAccountNo ?? string.Empty,
                    SettlementAccountName = sch.SettlementAccountName ?? string.Empty,
                }
            ).FirstOrDefaultAsync();
        }

        public Task<BillingInvoice?> GetByIdAsync(long invoiceId, CancellationToken ct)
        {
            return _context.Invoices
                .FirstOrDefaultAsync(i => i.InvoiceId == invoiceId, ct);
        }

        private async Task<int> CalculateHolidaysAsync(Guid schoolId)
        {
            var setting = await _context.SchoolPaymentSettings
                .Where(s => s.SchoolId == schoolId && s.IsActive)
                .FirstOrDefaultAsync();

            if (setting == null) return 0;

            int prevMonth = setting.FromMonth - 1;
            int year = DateTime.Now.Year;

            if (prevMonth <= 0)
            {
                prevMonth = 12;
                year -= 1;
            }

            var prevMonthFrom = DateOnly.FromDateTime(new DateTime(year, prevMonth, 1));
            var prevMonthTo =
                DateOnly.FromDateTime(prevMonthFrom.ToDateTime(TimeOnly.MinValue).AddMonths(1).AddDays(-1));

            return await (
                from dm in _context.DailyMeals
                join sm in _context.ScheduleMeals
                    on dm.ScheduleMealId equals sm.ScheduleMealId
                where
                    sm.SchoolId == schoolId &&
                    dm.MealDate >= prevMonthFrom &&
                    dm.MealDate <= prevMonthTo &&
                    dm.Notes != null
                select dm.DailyMealId
            ).CountAsync();
        }
    }
}
