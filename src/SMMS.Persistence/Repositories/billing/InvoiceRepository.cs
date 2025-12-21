using Microsoft.EntityFrameworkCore;
using SMMS.Application.Features.billing.DTOs;
using SMMS.Application.Features.billing.Interfaces;
using SMMS.Persistence.Data;
using SMMS.Domain.Entities.billing;
using BillingInvoice = SMMS.Domain.Entities.billing.Invoice;
using SMMS.Domain.Entities.school;

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
            var student = await _context.Students
                .AsNoTracking()
                .Where(s => s.StudentId == studentId)
                .Select(s => new { s.SchoolId, s.FullName })
                .FirstOrDefaultAsync();

            if (student == null) return Enumerable.Empty<InvoiceDto>();

            var invoices = await _context.Invoices
                .AsNoTracking()
                .Where(inv => inv.StudentId == studentId && inv.Status == "Unpaid")
                .ToListAsync();

            if (!invoices.Any()) return Enumerable.Empty<InvoiceDto>();

            var minDate = invoices.Min(i => new DateOnly(i.DateFrom.Year, i.DateFrom.Month, 1).AddMonths(-1));
            var maxDate = invoices.Max(i => i.DateFrom);

            var allSettings = await _context.SchoolPaymentSettings
                .AsNoTracking()
                .Where(s => s.SchoolId == student.SchoolId && s.IsActive)
                .ToListAsync();
         
            var allMealStats = await (from dm in _context.DailyMeals
                    join sm in _context.ScheduleMeals on dm.ScheduleMealId equals sm.ScheduleMealId
                    where sm.SchoolId == student.SchoolId &&
                          dm.MealDate >= minDate && dm.MealDate <= maxDate
                    select new { dm.MealDate, IsHoliday = dm.Notes != null })
                .Distinct()
                .ToListAsync();

            var allAttendances = await _context.Attendances
                .AsNoTracking()
                .Where(a => a.StudentId == studentId && a.AbsentDate >= minDate && a.AbsentDate <= maxDate)
                .Select(a => a.AbsentDate)
                .Distinct()
                .ToListAsync();

            var result = new List<InvoiceDto>();
            foreach (var inv in invoices)
            {
                //// 👉 Setting tháng trước
                var prevMonthDate = inv.DateFrom.AddMonths(-1);

                var prevMonthSetting = await _context.SchoolPaymentSettings
                    .AsNoTracking()
                    .FirstOrDefaultAsync(s =>
                        s.SchoolId == student.SchoolId &&
                        s.FromMonth == prevMonthDate.Month &&
                        s.IsActive);
                var stats = CalculateStats(inv.DateFrom, student.SchoolId, allSettings, allMealStats, allAttendances);

                result.Add(new InvoiceDto
                {
                    InvoiceId = inv.InvoiceId,
                    InvoiceCode = inv.InvoiceCode,
                    StudentName = student.FullName,
                    MonthNo = inv.MonthNo,
                    DateFrom = inv.DateFrom.ToDateTime(TimeOnly.MinValue),
                    DateTo = inv.DateTo.ToDateTime(TimeOnly.MinValue),
                    AbsentDay = inv.AbsentDay,
                    Holiday = stats.HolidayCount,
                    Status = inv.Status,
                    MealPricePerDayLastMonth = prevMonthSetting != null? prevMonthSetting.MealPricePerDay: 0,
                    MealPricePerDay = stats.Setting?.MealPricePerDay ?? 0,
                    AmountTotal = stats.Setting?.TotalAmount ?? 0,
                    TotalMealLastMonth = stats.ValidMealDays,
                    AmountToPay = inv.TotalPrice
                });
            }

            return result;
        }

        public async Task<InvoiceDetailDto?> GetInvoiceDetailAsync(long invoiceId, Guid studentId)
        {
            var inv = await _context.Invoices.AsNoTracking()
                .FirstOrDefaultAsync(i => i.InvoiceId == invoiceId && i.StudentId == studentId);
            if (inv == null) return null;

            var studentInfo = await (from stu in _context.Students
                join scCls in _context.StudentClasses on stu.StudentId equals scCls.StudentId
                join cls in _context.Classes on scCls.ClassId equals cls.ClassId
                join sch in _context.Schools on stu.SchoolId equals sch.SchoolId
                where stu.StudentId == studentId && scCls.LeftDate == null
                select new { stu, cls.ClassName, sch }).FirstOrDefaultAsync();

            if (studentInfo == null) return null;

            var firstDay = new DateOnly(inv.DateFrom.Year, inv.DateFrom.Month, 1);
            var prevMonthFrom = firstDay.AddMonths(-1);
            var prevMonthTo = firstDay.AddDays(-1);

            var settings = await _context.SchoolPaymentSettings
                .Where(s => s.SchoolId == studentInfo.stu.SchoolId && s.IsActive).ToListAsync();
            // Lấy setting của tháng trước
            var prevMonthDate = inv.DateFrom.AddMonths(-1);

            var prevMonthSetting = await _context.SchoolPaymentSettings
            .AsNoTracking()
                .FirstOrDefaultAsync(s =>
                    s.SchoolId == studentInfo.stu.SchoolId &&
                    s.FromMonth == prevMonthDate.Month &&
                    s.IsActive);
            var mealStats = await (from dm in _context.DailyMeals
                join sm in _context.ScheduleMeals on dm.ScheduleMealId equals sm.ScheduleMealId
                where sm.SchoolId == studentInfo.stu.SchoolId && dm.MealDate >= prevMonthFrom &&
                      dm.MealDate <= prevMonthTo
                select new { dm.MealDate, IsHoliday = dm.Notes != null }).Distinct().ToListAsync();
            var attendances = await _context.Attendances.Where(a =>
                    a.StudentId == studentId && a.AbsentDate >= prevMonthFrom && a.AbsentDate <= prevMonthTo)
                .Select(a => a.AbsentDate).ToListAsync();

            var stats = CalculateStats(inv.DateFrom, studentInfo.stu.SchoolId, settings,
                mealStats.Select(x => (object)x).Cast<dynamic>().ToList(), attendances);

            return new InvoiceDetailDto
            {
                InvoiceId = inv.InvoiceId,
                InvoiceCode = inv.InvoiceCode,
                StudentName = studentInfo.stu.FullName,
                ClassName = studentInfo.ClassName,
                SchoolName = studentInfo.sch.SchoolName,
                MonthNo = inv.MonthNo,
                DateFrom = inv.DateFrom.ToDateTime(TimeOnly.MinValue),
                DateTo = inv.DateTo.ToDateTime(TimeOnly.MinValue),
                AbsentDay = inv.AbsentDay,
                Holiday = stats.HolidayCount,
                Status = inv.Status,
                MealPricePerDay = stats.Setting?.MealPricePerDay ?? 0,
                MealPricePerDayLastMonth = prevMonthSetting != null? prevMonthSetting.MealPricePerDay: 0,
                TotalMealLastMonth = stats.ValidMealDays,
                AmountToPay = inv.TotalPrice,
                AmountTotal = stats.Setting?.TotalAmount ?? 0,
                SettlementBankCode = studentInfo.sch.SettlementBankCode ?? string.Empty,
                SettlementAccountNo = studentInfo.sch.SettlementAccountNo ?? string.Empty,
                SettlementAccountName = studentInfo.sch.SettlementAccountName ?? string.Empty,
            };
        }

        private (int AbsentCount, int HolidayCount, int ValidMealDays, SchoolPaymentSetting? Setting)
            CalculateStats(DateOnly dateFrom, Guid schoolId, List<SchoolPaymentSetting> settings, dynamic mealDates,
                List<DateOnly> attendances)
        {
            var firstDayOfCurrentMonth = new DateOnly(dateFrom.Year, dateFrom.Month, 1);
            var prevMonthFrom = firstDayOfCurrentMonth.AddMonths(-1);
            var prevMonthTo = firstDayOfCurrentMonth.AddDays(-1);

            var setting = settings.FirstOrDefault(s => s.FromMonth == dateFrom.Month);

            var monthlyMeals = ((IEnumerable<dynamic>)mealDates)
                .Where(m => m.MealDate >= prevMonthFrom && m.MealDate <= prevMonthTo).ToList();

            var holidayCount = monthlyMeals.Count(m => m.IsHoliday);
            var plannedMealDates = monthlyMeals.Where(m => !m.IsHoliday).Select(m => (DateOnly)m.MealDate).ToList();

            var absentInMealDays = attendances
                .Count(a => a >= prevMonthFrom && a <= prevMonthTo && plannedMealDates.Contains(a));

            int validMealDays = Math.Max(0, plannedMealDates.Count - absentInMealDays);

            return (absentInMealDays, holidayCount, validMealDays, setting);
        }

        public Task<BillingInvoice?> GetByIdAsync(long invoiceId, CancellationToken ct)
        {
            return _context.Invoices.FirstOrDefaultAsync(i => i.InvoiceId == invoiceId, ct);
        }
    }
}
