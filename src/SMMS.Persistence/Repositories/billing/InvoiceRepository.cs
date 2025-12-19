using Microsoft.EntityFrameworkCore;
using PayOS.Models.V2.PaymentRequests.Invoices;
using SMMS.Application.Features.billing.DTOs;
using SMMS.Application.Features.billing.Interfaces;
using SMMS.Domain.Entities.billing;
using SMMS.Domain.Entities.school;
using SMMS.Persistence.Data;
using static System.Runtime.InteropServices.JavaScript.JSType;
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
                    Holiday = 0,
                    Status = inv.Status,
                    MealPricePerDay = 0,
                    AmountToPay = inv.TotalPrice
                };
            var invList = await query.ToListAsync() ?? new List<InvoiceDto>();
            foreach (InvoiceDto invoice in invList){
                var setting = await _context.SchoolPaymentSettings
               .Where(s => s.SchoolId == schoolId && s.FromMonth == invoice.DateFrom.Month && s.IsActive)
               .FirstOrDefaultAsync();
                if (setting == null)
                {
                    throw new Exception("Không tìm thấy payment setting");
                }
                decimal MealPrice = setting.MealPricePerDay;
                invoice.MealPricePerDay = MealPrice;

                var prevMonthFrom = DateOnly.FromDateTime(invoice.DateFrom.AddMonths(-1));
                var prevMonthTo = DateOnly.FromDateTime(invoice.DateFrom.AddDays(-1));

                // Đếm số ngày nghỉ trong tháng trước
                int holidayCount = await (
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
                invoice.Holiday = holidayCount;
            }
;
            return invList;
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

            var invoiceInfo = await _context.Invoices
                .Where(i => i.InvoiceId == invoiceId && i.StudentId == studentId)
                .Select(i => new { i.DateFrom, i.DateTo })
                .FirstOrDefaultAsync();
        //
            var setting = await _context.SchoolPaymentSettings
            .Where(s => s.SchoolId == schoolId && s.FromMonth == invoiceInfo.DateFrom.Month && s.IsActive)
             .FirstOrDefaultAsync();
            // Đếm số ngày nghỉ của tháng trước

                // Tính tháng trước
                int prevMonth = invoiceInfo.DateFrom.Month - 1;
                int year = invoiceInfo.DateFrom.Year;

                if (prevMonth <= 0)
                {
                    prevMonth = 12; // lùi về tháng 12 của năm trước
                    year -= 1;
                }

                // Ngày đầu và cuối của tháng trước
                var prevMonthFrom = DateOnly.FromDateTime(new DateTime(year, prevMonth, 1));
                var prevMonthTo = DateOnly.FromDateTime(prevMonthFrom.ToDateTime(TimeOnly.MinValue).AddMonths(1).AddDays(-1));

                // Đếm số ngày nghỉ trong tháng trước
               int holidayCount = await (
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
                    Holiday = holidayCount,
                    Status = inv.Status,
                    MealPricePerDay= setting.MealPricePerDay,
                    // Số tiền phải đóng
                    AmountToPay = inv.TotalPrice,
                    // 🏦 Thông tin ngân hàng của trường
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
    }
}
