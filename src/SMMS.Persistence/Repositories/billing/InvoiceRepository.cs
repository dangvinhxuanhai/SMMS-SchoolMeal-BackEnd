using Microsoft.EntityFrameworkCore;
using SMMS.Application.Features.billing.DTOs;
using SMMS.Application.Features.billing.Interfaces;
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

        public async Task<IEnumerable<InvoiceDto>> GetInvoicesByParentAsync(Guid studentId)
        {
            return await _context.Invoices
                .AsNoTracking()
                .Where(inv => inv.StudentId == studentId)
                .OrderByDescending(inv => inv.DateFrom)
                .Select(inv => new InvoiceDto
                {
                    InvoiceId = inv.InvoiceId,
                    InvoiceCode = inv.InvoiceCode,
                    StudentName = inv.Student.FullName,
                    MonthNo = inv.MonthNo,
                    DateFrom = inv.DateFrom.ToDateTime(TimeOnly.MinValue),
                    DateTo = inv.DateTo.ToDateTime(TimeOnly.MinValue),
                    AbsentDay = inv.AbsentDay,
                    Status = inv.Status,
                    AmountToPay = inv.TotalPrice
                })
                .ToListAsync();
        }

        public async Task<IEnumerable<InvoiceDto>> GetUnpaidInvoicesAsync(Guid studentId)
        {
            var studentInfo = await _context.Students
                .AsNoTracking()
                .Where(s => s.StudentId == studentId)
                .Select(s => new { s.SchoolId, s.FullName })
                .FirstOrDefaultAsync();

            if (studentInfo == null) return Enumerable.Empty<InvoiceDto>();

            var invoices = await _context.Invoices
                .AsNoTracking()
                .Where(inv => inv.StudentId == studentId && inv.Status == "Unpaid")
                .ToListAsync();

            if (!invoices.Any()) return Enumerable.Empty<InvoiceDto>();

            var result = new List<InvoiceDto>();

            foreach (var inv in invoices)
            {
                var prevMonthFrom = inv.DateFrom.AddMonths(-1);
                var prevMonthTo = inv.DateFrom.AddDays(-1);

                var setting = await _context.SchoolPaymentSettings
                    .AsNoTracking()
                    .FirstOrDefaultAsync(s =>
                        s.SchoolId == studentInfo.SchoolId && s.FromMonth == inv.DateFrom.Month && s.IsActive);

                var mealStats = await (from dm in _context.DailyMeals
                        join sm in _context.ScheduleMeals on dm.ScheduleMealId equals sm.ScheduleMealId
                        where sm.SchoolId == studentInfo.SchoolId &&
                              dm.MealDate >= prevMonthFrom &&
                              dm.MealDate <= prevMonthTo
                        select new { dm.MealDate, IsHoliday = dm.Notes != null })
                    .Distinct()
                    .ToListAsync();
                // So don xin nghi thang truoc
                int absentRequestCount = await _context.Attendances
                .Where(a =>
                a.StudentId == studentId &&
                a.AbsentDate >= prevMonthFrom &&
                a.AbsentDate <= prevMonthTo
                )
                .Select(a => a.AbsentDate)
                .Distinct()
                .CountAsync();
                int holidayCount = mealStats.Count(x => x.IsHoliday);
                int validMealDays = mealStats.Count(x => !x.IsHoliday)- absentRequestCount;

                result.Add(new InvoiceDto
                {
                    InvoiceId = inv.InvoiceId,
                    InvoiceCode = inv.InvoiceCode,
                    StudentName = studentInfo.FullName,
                    MonthNo = inv.MonthNo,
                    DateFrom = inv.DateFrom.ToDateTime(TimeOnly.MinValue),
                    DateTo = inv.DateTo.ToDateTime(TimeOnly.MinValue),
                    AbsentDay = absentRequestCount,
                    Holiday = holidayCount,
                    Status = inv.Status,
                    MealPricePerDay = setting?.MealPricePerDay ?? 0,
                    AmountTotal = setting?.TotalAmount ?? 0,
                    TotalMealLastMonth = Math.Max(0, validMealDays),
                    AmountToPay = inv.TotalPrice
                });
            }

            return result;
        }

        public async Task<InvoiceDetailDto?> GetInvoiceDetailAsync(long invoiceId, Guid studentId)
        {
            var invoiceData = await _context.Invoices
                .AsNoTracking()
                .Where(i => i.InvoiceId == invoiceId && i.StudentId == studentId)
                .Select(inv => new
                {
                    inv,
                    Student = _context.Students.FirstOrDefault(s => s.StudentId == inv.StudentId),
                    PrevMonthFrom = inv.DateFrom.AddMonths(-1),
                    PrevMonthTo = inv.DateFrom.AddDays(-1)
                })
                .FirstOrDefaultAsync();

            if (invoiceData == null) return null;

            var schoolId = invoiceData.Student.SchoolId;

            var setting = await _context.SchoolPaymentSettings
                .AsNoTracking()
                .FirstOrDefaultAsync(s =>
                    s.SchoolId == schoolId && s.FromMonth == invoiceData.inv.DateFrom.Month && s.IsActive);
            // So don xin nghi thang truoc
            int absentRequestCount = await _context.Attendances
            .Where(a =>
            a.StudentId == studentId &&
            a.AbsentDate >= invoiceData.PrevMonthFrom &&
            a.AbsentDate <= invoiceData.PrevMonthTo
            )
            .Select(a => a.AbsentDate)
            .Distinct()
            .CountAsync();
            var mealStats = await (from dm in _context.DailyMeals
                    join sm in _context.ScheduleMeals on dm.ScheduleMealId equals sm.ScheduleMealId
                    where sm.SchoolId == schoolId &&
                          dm.MealDate >= invoiceData.PrevMonthFrom &&
                          dm.MealDate <= invoiceData.PrevMonthTo
                    select new { dm.MealDate, IsHoliday = dm.Notes != null })
                .Distinct()
                .ToListAsync();

            int holidayCount = mealStats.Count(x => x.IsHoliday);
            int validMealDays = mealStats.Count(x => !x.IsHoliday)-absentRequestCount;

            return await (from inv in _context.Invoices
                join stu in _context.Students on inv.StudentId equals stu.StudentId
                join scCls in _context.StudentClasses on stu.StudentId equals scCls.StudentId
                join cls in _context.Classes on scCls.ClassId equals cls.ClassId
                join sch in _context.Schools on stu.SchoolId equals sch.SchoolId
                where inv.InvoiceId == invoiceId && scCls.LeftDate == null
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
                    AbsentDay = absentRequestCount,
                    Holiday = holidayCount,
                    Status = inv.Status,
                    MealPricePerDay = setting != null ? setting.MealPricePerDay : 0,
                    TotalMealLastMonth = Math.Max(0, validMealDays),
                    AmountToPay = inv.TotalPrice,
                    AmountTotal = setting != null ? setting.TotalAmount : 0,
                    SettlementBankCode = sch.SettlementBankCode ?? string.Empty,
                    SettlementAccountNo = sch.SettlementAccountNo ?? string.Empty,
                    SettlementAccountName = sch.SettlementAccountName ?? string.Empty,
                }).FirstOrDefaultAsync();
        }

        public Task<BillingInvoice?> GetByIdAsync(long invoiceId, CancellationToken ct)
        {
            return _context.Invoices
                .FirstOrDefaultAsync(i => i.InvoiceId == invoiceId, ct);
        }
    }
}
