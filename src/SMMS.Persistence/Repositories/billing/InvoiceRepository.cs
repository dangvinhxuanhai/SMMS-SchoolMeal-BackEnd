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
                .Select(inv => new
                {
                    Invoice = inv,
                    MealPrice = _context.SchoolPaymentSettings
                        .Where(ps =>
                            ps.SchoolId == student.SchoolId && ps.FromMonth == inv.DateFrom.Month && ps.IsActive)
                        .Select(ps => ps.MealPricePerDay)
                        .FirstOrDefault(),
                    // Subquery đếm số ngày nghỉ tháng trước (Holiday)
                    HolidayCount = _context.DailyMeals
                        .Count(dm => _context.ScheduleMeals
                            .Any(sm => sm.ScheduleMealId == dm.ScheduleMealId &&
                                       sm.SchoolId == student.SchoolId &&
                                       dm.MealDate >= inv.DateFrom.AddMonths(-1) &&
                                       dm.MealDate < inv.DateFrom &&
                                       dm.Notes != null))
                })
                .ToListAsync();

            return invoices.Select(x => new InvoiceDto
            {
                InvoiceId = x.Invoice.InvoiceId,
                InvoiceCode = x.Invoice.InvoiceCode,
                StudentName = student.FullName,
                MonthNo = x.Invoice.MonthNo,
                DateFrom = x.Invoice.DateFrom.ToDateTime(TimeOnly.MinValue),
                DateTo = x.Invoice.DateTo.ToDateTime(TimeOnly.MinValue),
                AbsentDay = x.Invoice.AbsentDay,
                Holiday = x.HolidayCount,
                Status = x.Invoice.Status,
                MealPricePerDay = x.MealPrice,
                AmountToPay = x.Invoice.TotalPrice
            });
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

        public async Task<InvoiceDetailDto?> GetInvoiceDetailAsync(long invoiceId, Guid studentId)
        {
            var query = _context.Invoices
                .AsNoTracking()
                .Where(i => i.InvoiceId == invoiceId && i.StudentId == studentId)
                .Select(inv => new
                {
                    Inv = inv,
                    Student = inv.Student,
                    School = inv.Student.School,
                    ClassName = inv.Student.StudentClasses
                        .Where(sc => sc.LeftDate == null)
                        .Select(sc => sc.Class.ClassName)
                        .FirstOrDefault(),
                    MealPrice = _context.SchoolPaymentSettings
                        .Where(ps =>
                            ps.SchoolId == inv.Student.SchoolId && ps.FromMonth == inv.DateFrom.Month && ps.IsActive)
                        .Select(ps => ps.MealPricePerDay)
                        .FirstOrDefault(),
                    HolidayCount = _context.DailyMeals
                        .Count(dm => _context.ScheduleMeals
                            .Any(sm => sm.ScheduleMealId == dm.ScheduleMealId &&
                                       sm.SchoolId == inv.Student.SchoolId &&
                                       dm.MealDate >= inv.DateFrom.AddMonths(-1) &&
                                       dm.MealDate < inv.DateFrom &&
                                       dm.Notes != null))
                });

            var result = await query.FirstOrDefaultAsync();

            if (result == null) return null;

            return new InvoiceDetailDto
            {
                InvoiceId = result.Inv.InvoiceId,
                InvoiceCode = result.Inv.InvoiceCode,
                StudentName = result.Student.FullName,
                ClassName = result.ClassName ?? "N/A",
                SchoolName = result.School.SchoolName,
                MonthNo = result.Inv.MonthNo,
                DateFrom = result.Inv.DateFrom.ToDateTime(TimeOnly.MinValue),
                DateTo = result.Inv.DateTo.ToDateTime(TimeOnly.MinValue),
                AbsentDay = result.Inv.AbsentDay,
                Holiday = result.HolidayCount,
                Status = result.Inv.Status,
                MealPricePerDay = result.MealPrice,
                AmountToPay = result.Inv.TotalPrice,
                SettlementBankCode = result.School.SettlementBankCode ?? string.Empty,
                SettlementAccountNo = result.School.SettlementAccountNo ?? string.Empty,
                SettlementAccountName = result.School.SettlementAccountName ?? string.Empty,
            };
        }

        public async Task<BillingInvoice?> GetByIdAsync(long invoiceId, CancellationToken ct)
        {
            return await _context.Invoices
                .FirstOrDefaultAsync(i => i.InvoiceId == invoiceId, ct);
        }
    }
}
