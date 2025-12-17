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
using SMMS.Application.Common.Helpers;
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
    public async Task<int> CountHolidayMealDaysAsync(
       Guid schoolId,
       DateOnly dateFrom,
       DateOnly dateTo,
       CancellationToken ct)
    {
        return await (
            from dm in _context.DailyMeals
            join sm in _context.ScheduleMeals on dm.ScheduleMealId equals sm.ScheduleMealId
            where sm.SchoolId == schoolId
                  && dm.MealDate >= dateFrom
                  && dm.MealDate <= dateTo
                  && dm.Notes != null
            select dm.MealDate
        ).Distinct().CountAsync(ct);
    }
    public async Task<IReadOnlyList<ExportStudentFeeRowDto>> GetExportFeeBoardRowsAsync(
        Guid schoolId, short monthNo, int year, Guid? classId, CancellationToken ct)
    {
        // tháng trước
        short prevMonth = (short)(monthNo == 1 ? 12 : monthNo - 1);
        int prevYear = (monthNo == 1 ? year - 1 : year);

        var (prevFrom, prevTo) = DateOnlyUtils.GetMonthRange(prevYear, prevMonth);

        // 1) holidayPrev (school-wide) tháng trước
        int holidayPrev = await CountHolidayMealDaysAsync(schoolId, prevFrom, prevTo, ct);

        // 2) absentPrevStudent tháng trước (theo từng học sinh)
        var absentPrevMap = await _context.Attendances.AsNoTracking()
            .Where(a => a.AbsentDate >= prevFrom && a.AbsentDate <= prevTo)
            .Join(_context.Students.AsNoTracking(),
                a => a.StudentId,
                s => s.StudentId,
                (a, s) => new { a.StudentId, s.SchoolId, a.AbsentDate })
            .Where(x => x.SchoolId == schoolId)
            .GroupBy(x => x.StudentId)
            .ToDictionaryAsync(
                g => g.Key,
                g => g.Select(x => x.AbsentDate).Distinct().Count(),
                ct);

        // 3) MealPricePerDay tháng trước
        var prevPerDay = await _context.SchoolPaymentSettings.AsNoTracking()
            .Where(x => x.SchoolId == schoolId && x.FromMonth == prevMonth)
            .OrderByDescending(x => x.CreatedAt)
            .Select(x => (decimal?)x.MealPricePerDay)
            .FirstOrDefaultAsync(ct) ?? 0m;

        // 4) TotalAmount tháng hiện tại (tháng export)
        var curTotalAmount = await _context.SchoolPaymentSettings.AsNoTracking()
            .Where(x => x.SchoolId == schoolId && x.FromMonth == monthNo)
            .OrderByDescending(x => x.CreatedAt)
            .Select(x => (decimal?)x.TotalAmount)
            .FirstOrDefaultAsync(ct) ?? 0m;

        // 5) Invoice tháng hiện tại
        var query =
            from i in _context.Invoices.AsNoTracking()
            join s in _context.Students.AsNoTracking() on i.StudentId equals s.StudentId
            where s.SchoolId == schoolId
                  && i.MonthNo == monthNo
                  && i.DateFrom.Year == year
            select new { i, s };

        // nếu có classId thì filter ở đây (tùy schema)
        // if (classId.HasValue) query = query.Where(x => x.s.ClassId == classId.Value);

        var rows = await query
            .OrderBy(x => x.s.FullName)
            .Select(x => new ExportStudentFeeRowDto
            {
                StudentId = x.s.StudentId,
                StudentName = x.s.FullName,

                // Cột 1: prevPerDay * (holidayPrev + absentPrevStudent)
                PrevDeduction =
                    prevPerDay * (holidayPrev + (absentPrevMap.ContainsKey(x.s.StudentId) ? absentPrevMap[x.s.StudentId] : 0)),

                // Cột 2: TotalAmount trong SchoolPaymentSettings tháng hiện tại
                SettingTotalAmount = curTotalAmount,

                // Cột 3: TotalPrice trong Invoice
                InvoiceTotalPrice = (decimal?)x.i.TotalPrice ?? 0m
            })
            .ToListAsync(ct);

        for (int idx = 0; idx < rows.Count; idx++)
            rows[idx].No = idx + 1;

        return rows;
    }



    // optional
    public Task<string?> GetSchoolNameAsync(Guid schoolId, CancellationToken ct)
        => _context.Schools.AsNoTracking()
            .Where(x => x.SchoolId == schoolId)
            .Select(x => x.SchoolName)   // ✅ đúng column
            .FirstOrDefaultAsync(ct);

    public Task<string?> GetClassNameAsync(Guid classId, CancellationToken ct)
        => _context.Classes.AsNoTracking()
            .Where(x => x.ClassId == classId)
            .Select(x => x.ClassName)    // ✅ đúng column
            .FirstOrDefaultAsync(ct);
}
