using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ClosedXML.Excel;
using DocumentFormat.OpenXml.InkML;
using MediatR;
using Microsoft.EntityFrameworkCore;
using SMMS.Application.Common.Helpers;
using SMMS.Application.Features.Manager.DTOs;
using SMMS.Application.Features.Manager.Interfaces;
using SMMS.Application.Features.Manager.Queries;
using SMMS.Domain.Entities.billing;
using SMMS.Domain.Entities.school;

namespace SMMS.Application.Features.Manager.Handlers;
public class GetSchoolInvoicesHandler :
    IRequestHandler<GetSchoolInvoicesQuery, IReadOnlyList<InvoiceDto1>>
{
    private readonly ISchoolInvoiceRepository _repo;

    public GetSchoolInvoicesHandler(ISchoolInvoiceRepository repo)
    {
        _repo = repo;
    }

    public async Task<IReadOnlyList<InvoiceDto1>> Handle(
        GetSchoolInvoicesQuery request,
        CancellationToken ct)
    {
        // Bắt buộc để tính "tháng trước"
        if (!request.MonthNo.HasValue || !request.Year.HasValue)
            throw new ArgumentException("Cần truyền MonthNo và Year để tính AbsentDay = holidayPrev + absentPrevStudent (tháng trước).");

        var monthNo = request.MonthNo.Value;
        var year = request.Year.Value;

        // tháng trước
        short prevMonth = (short)(monthNo == 1 ? 12 : monthNo - 1);
        int prevYear = (monthNo == 1 ? year - 1 : year);

        var prevFrom = new DateOnly(prevYear, prevMonth, 1);
        var prevTo = new DateOnly(prevYear, prevMonth, DateTime.DaysInMonth(prevYear, prevMonth));

        // ✅ holidayPrev theo school (tháng trước)
        var holidayPrev = await _repo.CountHolidayMealDaysAsync(request.SchoolId, prevFrom, prevTo, ct);

        // ✅ absentPrevStudent map theo student (tháng trước)
        var absentPrevMap = await _repo.CountAbsentDaysByStudentAsync(request.SchoolId, prevFrom, prevTo, ct);

        // Query invoice tháng hiện tại
        var query =
            from i in _repo.Invoices
            join s in _repo.Students on i.StudentId equals s.StudentId
            where s.SchoolId == request.SchoolId
                  && i.MonthNo == monthNo
                  && i.DateFrom.Year == year
            select new { i, s };

        if (!string.IsNullOrWhiteSpace(request.Status))
            query = query.Where(x => x.i.Status == request.Status);

        return await query
            .Select(x => new InvoiceDto1
            {
                InvoiceId = x.i.InvoiceId,
                InvoiceCode = x.i.InvoiceCode,
                StudentId = x.i.StudentId,
                StudentName = x.s.FullName,
                MonthNo = x.i.MonthNo,
                DateFrom = x.i.DateFrom.ToDateTime(TimeOnly.MinValue),
                DateTo = x.i.DateTo.ToDateTime(TimeOnly.MinValue),

                // ✅ AbsentDay theo yêu cầu: holidayPrev + absentPrevStudent (tháng trước)
                AbsentDay = holidayPrev + (absentPrevMap.ContainsKey(x.i.StudentId)
                                ? absentPrevMap[x.i.StudentId]
                                : 0),

                Status = x.i.Status,
                TotalPrice = x.i.TotalPrice
            })
            .ToListAsync(ct);
    }
}


public class GetSchoolInvoiceByIdHandler :
    IRequestHandler<GetSchoolInvoiceByIdQuery, InvoiceDto1?>
{
    private readonly ISchoolInvoiceRepository _repo;

    public GetSchoolInvoiceByIdHandler(ISchoolInvoiceRepository repo)
    {
        _repo = repo;
    }

    public async Task<InvoiceDto1?> Handle(
        GetSchoolInvoiceByIdQuery request,
        CancellationToken cancellationToken)
    {
        var row = await (
            from i in _repo.Invoices
            join s in _repo.Students on i.StudentId equals s.StudentId
            where i.InvoiceId == request.InvoiceId &&
                  s.SchoolId == request.SchoolId
            select new { i, s }
        ).FirstOrDefaultAsync(cancellationToken);

        if (row == null) return null;

        return new InvoiceDto1
        {
            InvoiceId = row.i.InvoiceId,
            InvoiceCode = row.i.InvoiceCode,
            StudentId = row.i.StudentId,
            StudentName = row.s.FullName, // ✅ thêm
            MonthNo = row.i.MonthNo,
            DateFrom = row.i.DateFrom.ToDateTime(TimeOnly.MinValue),
            DateTo = row.i.DateTo.ToDateTime(TimeOnly.MinValue),
            AbsentDay = row.i.AbsentDay,
            Status = row.i.Status,
            TotalPrice = row.i.TotalPrice // ✅ thêm
        };
    }
}

public class GenerateSchoolInvoicesHandler
    : IRequestHandler<GenerateSchoolInvoicesCommand, IReadOnlyList<InvoiceDto1>>
{
    private readonly ISchoolInvoiceRepository _repo;
    private readonly IManagerPaymentSettingRepository _paymentRepo;

    public GenerateSchoolInvoicesHandler(ISchoolInvoiceRepository repo, IManagerPaymentSettingRepository paymentRepo)
    {
        _repo = repo;
        _paymentRepo = paymentRepo;
    }

    public async Task<IReadOnlyList<InvoiceDto1>> Handle(GenerateSchoolInvoicesCommand request, CancellationToken ct)
    {
        var dtFrom = request.Request.DateFrom.Date;
        var dtTo = request.Request.DateTo.Date;

        if (dtFrom > dtTo) throw new ArgumentException("DateFrom must be <= DateTo.");
        if (dtFrom.Year != dtTo.Year) throw new ArgumentException("Không được tạo invoice cho nhiều năm khác nhau.");

        short monthNo = (short)dtFrom.Month;

        // 1) setting tháng hiện tại
        var settingCur = await _paymentRepo.GetByMonthAsync(request.SchoolId, monthNo, ct);
        if (settingCur == null)
            throw new InvalidOperationException("Tháng này chưa có payment setting.");

        var fromD = DateOnly.FromDateTime(dtFrom);
        var toD = DateOnly.FromDateTime(dtTo);

        var weekdayCur = DateOnlyUtils.CountWeekdays(fromD, toD);
        var baseAmount = settingCur.MealPricePerDay * weekdayCur;

        // 2) dữ liệu tháng trước
        decimal prevPerDay = 0;
        int holidayPrev = 0;
        Dictionary<Guid, int> absentPrevMap = new();

        if (monthNo > 1)
        {
            short prevMonth = (short)(monthNo - 1);

            var settingPrev = await _paymentRepo.GetByMonthAsync(request.SchoolId, prevMonth, ct);

            if (settingPrev != null)
            {
                prevPerDay = settingPrev.MealPricePerDay;

                var (prevFrom, prevTo) = DateOnlyUtils.GetMonthRange(dtFrom.Year, prevMonth);

                holidayPrev = await _repo.CountHolidayMealDaysAsync(request.SchoolId, prevFrom, prevTo, ct);

                absentPrevMap = await _repo.Attendance
                    .Where(a => a.AbsentDate >= prevFrom && a.AbsentDate <= prevTo)
                    .Join(_repo.Students,
                        a => a.StudentId,
                        s => s.StudentId,
                        (a, s) => new { a.StudentId, s.SchoolId, a.AbsentDate })
                    .Where(x => x.SchoolId == request.SchoolId)
                    .GroupBy(x => x.StudentId)
                    .ToDictionaryAsync(
                        g => g.Key,
                        g => g.Select(x => x.AbsentDate).Distinct().Count(),
                        ct);
            }
        }

        // 3) học sinh active
        var students = await _repo.Students
            .Where(s => s.SchoolId == request.SchoolId && s.IsActive)
            .Select(s => s.StudentId)
            .ToListAsync(ct);

        if (!students.Any()) return Array.Empty<InvoiceDto1>();

        // 4) absent tháng hiện tại (lưu vào invoice.AbsentDay)
        var absentCurMap = await _repo.Attendance
            .Where(a => a.AbsentDate >= fromD && a.AbsentDate <= toD)
            .Join(_repo.Students,
                a => a.StudentId,
                s => s.StudentId,
                (a, s) => new { a.StudentId, s.SchoolId, a.AbsentDate })
            .Where(x => x.SchoolId == request.SchoolId)
            .GroupBy(x => x.StudentId)
            .ToDictionaryAsync(
                g => g.Key,
                g => g.Select(x => x.AbsentDate).Distinct().Count(),
                ct);

        // 5) invoice existed overlap
        var existedStudentIds = await _repo.Invoices
            .Where(i => students.Contains(i.StudentId) && i.DateFrom <= toD && i.DateTo >= fromD)
            .Select(i => i.StudentId)
            .Distinct()
            .ToListAsync(ct);

        var existed = existedStudentIds.ToHashSet();

        // 6) tạo invoices
        var newInvoices = new List<Invoice>();

        foreach (var sid in students)
        {
            if (request.Request.SkipExisting && existed.Contains(sid))
                continue;

            var absentCur = absentCurMap.TryGetValue(sid, out var c) ? c : 0;
            var absentPrev = 0;
            if (monthNo > 1)
                absentPrev = absentPrevMap.TryGetValue(sid, out var ap) ? ap : 0;
            decimal totalPrice = baseAmount;

            if (monthNo > 1)
            {
                totalPrice = baseAmount - (prevPerDay * (holidayPrev + absentPrev));
                if (totalPrice < 0) totalPrice = 0; // optional
            }

            newInvoices.Add(new Invoice
            {
                InvoiceCode = Guid.NewGuid(),
                StudentId = sid,
                MonthNo = monthNo,
                DateFrom = fromD,
                DateTo = toD,
                AbsentDay = absentCur + absentPrev,
                Status = "Unpaid",
                TotalPrice = totalPrice
            });
        }

        if (newInvoices.Any())
            await _repo.AddInvoicesAsync(newInvoices, ct);

        await _repo.SaveChangesAsync(ct);

        return newInvoices.Select(i => new InvoiceDto1
        {
            InvoiceId = i.InvoiceId,
            InvoiceCode = i.InvoiceCode,
            StudentId = i.StudentId,
            MonthNo = i.MonthNo,
            DateFrom = i.DateFrom.ToDateTime(TimeOnly.MinValue),
            DateTo = i.DateTo.ToDateTime(TimeOnly.MinValue),
            AbsentDay = i.AbsentDay,
            Status = i.Status,
            TotalPrice = i.TotalPrice
        }).ToList();
    }
}


public class UpdateInvoiceHandler :
    IRequestHandler<UpdateInvoiceCommand, InvoiceDto1?>
{
    private readonly ISchoolInvoiceRepository _repo;

    public UpdateInvoiceHandler(ISchoolInvoiceRepository repo)
    {
        _repo = repo;
    }

    public async Task<InvoiceDto1?> Handle(
        UpdateInvoiceCommand request,
        CancellationToken ct)
    {
        var invoice = await (
                from i in _repo.Invoices
                join s in _repo.Students on i.StudentId equals s.StudentId
                where i.InvoiceId == request.InvoiceId &&
                      s.SchoolId == request.SchoolId
                select i)
            .FirstOrDefaultAsync(ct);

        if (invoice == null)
            return null;

        var dtFrom = request.Request.DateFrom.Date;
        var dtTo = request.Request.DateTo.Date;

        // 1️⃣ Validate cơ bản
        if (dtFrom > dtTo)
            throw new ArgumentException("DateFrom must be <= DateTo.");

        short fromMonth = (short)dtFrom.Month;
        short toMonth = (short)dtTo.Month;

        if (fromMonth < 1 || fromMonth > 12 ||
            toMonth < 1 || toMonth > 12)
        {
            throw new ArgumentException("Tháng phải nằm trong khoảng từ 1 đến 12.");
        }

        if (dtFrom.Year != dtTo.Year)
        {
            throw new ArgumentException("Không được cập nhật invoice sang khoảng khác năm.");
        }

        var fromD = DateOnly.FromDateTime(dtFrom);
        var toD = DateOnly.FromDateTime(dtTo);

        // 2️⃣ Check chồng lấn với invoice khác của cùng học sinh
        bool overlapped = await _repo.Invoices
            .AnyAsync(i =>
                i.StudentId == invoice.StudentId &&
                i.InvoiceId != invoice.InvoiceId &&   // bỏ qua chính nó
                i.DateFrom <= toD &&
                i.DateTo >= fromD,
                ct);

        if (overlapped)
        {
            throw new InvalidOperationException(
                "Khoảng ngày này trùng với một invoice khác của học sinh.");
        }

        invoice.MonthNo = (short)dtFrom.Month;
        invoice.DateFrom = fromD;
        invoice.DateTo = toD;
        invoice.AbsentDay = request.Request.AbsentDay;
        invoice.Status = request.Request.Status;
        _repo.Update(invoice);
        await _repo.SaveChangesAsync(ct);

        return new InvoiceDto1
        {
            InvoiceId = invoice.InvoiceId,
            InvoiceCode = invoice.InvoiceCode,
            StudentId = invoice.StudentId,
            MonthNo = invoice.MonthNo,
            DateFrom = invoice.DateFrom.ToDateTime(TimeOnly.MinValue),
            DateTo = invoice.DateTo.ToDateTime(TimeOnly.MinValue),
            AbsentDay = invoice.AbsentDay,
            Status = invoice.Status
        };
    }
}

public class DeleteInvoiceHandler :
    IRequestHandler<DeleteInvoiceCommand, bool>
{
    private readonly ISchoolInvoiceRepository _repo;

    public DeleteInvoiceHandler(ISchoolInvoiceRepository repo)
    {
        _repo = repo;
    }

    public async Task<bool> Handle(DeleteInvoiceCommand request, CancellationToken ct)
    {
        var invoice = await (
                from i in _repo.Invoices
                join s in _repo.Students on i.StudentId equals s.StudentId
                where i.InvoiceId == request.InvoiceId &&
                      s.SchoolId == request.SchoolId
                select i)
            .FirstOrDefaultAsync(ct);

        if (invoice == null)
            return false;

        return await _repo.DeleteInvoiceAsync(invoice, ct);
    }
}
public class ExportSchoolFeeBoardHandler
    : IRequestHandler<ExportSchoolFeeBoardCommand, ExportFeeBoardResult>
{
    private readonly ISchoolInvoiceRepository _repo;

    public ExportSchoolFeeBoardHandler(ISchoolInvoiceRepository repo)
    {
        _repo = repo;
    }

    public async Task<ExportFeeBoardResult> Handle(ExportSchoolFeeBoardCommand request, CancellationToken ct)
    {
        var rows = await _repo.GetExportFeeBoardRowsAsync(
            request.SchoolId,
            request.MonthNo,
            request.Year,
            request.ClassId,
            ct);

        var schoolName = await _repo.GetSchoolNameAsync(request.SchoolId, ct)
                        ?? "TRƯỜNG TIỂU HỌC ...";

        var className = request.ClassId.HasValue
            ? (await _repo.GetClassNameAsync(request.ClassId.Value, ct) ?? "LỚP ...")
            : "DANH SÁCH";

        using var wb = new XLWorkbook();
        var ws = wb.Worksheets.Add("Bang thu");

        // ===== Title =====
        ws.Cell(1, 1).Value = schoolName;
        ws.Range(1, 1, 1, 5).Merge().Style.Font.SetBold().Font.SetFontSize(14);
        ws.Range(1, 1, 1, 5).Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);

        ws.Cell(2, 1).Value =
            $"DANH SÁCH HỌC SINH NỘP TIỀN DỊCH VỤ BÁN TRÚ THÁNG {request.MonthNo}/{request.Year}";
        ws.Range(2, 1, 2, 5).Merge().Style.Font.SetBold().Font.SetFontSize(12);
        ws.Range(2, 1, 2, 5).Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);

        ws.Cell(3, 1).Value = className;
        ws.Range(3, 1, 3, 5).Merge().Style.Font.SetBold();
        ws.Range(3, 1, 3, 5).Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);

        // ===== Header =====
        int headerRow = 5;
        ws.Cell(headerRow, 1).Value = "STT";
        ws.Cell(headerRow, 2).Value = "Họ tên học sinh";

        // Cột 1/2/3 theo yêu cầu
        ws.Cell(headerRow, 3).Value = "Tiền trừ tháng trước";
        ws.Cell(headerRow, 4).Value = "Tổng tiền tháng này";
        ws.Cell(headerRow, 5).Value = "Thành tiền";

        var header = ws.Range(headerRow, 1, headerRow, 5);
        header.Style.Font.SetBold();
        header.Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);
        header.Style.Alignment.SetVertical(XLAlignmentVerticalValues.Center);
        header.Style.Alignment.SetWrapText(true);
        header.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
        header.Style.Border.InsideBorder = XLBorderStyleValues.Thin;

        // ===== Data =====
        int r = headerRow + 1;
        foreach (var x in rows)
        {
            ws.Cell(r, 1).Value = x.No;
            ws.Cell(r, 2).Value = x.StudentName;

            // 3 cột mới
            ws.Cell(r, 3).Value = x.PrevDeduction;       // cột 1
            ws.Cell(r, 4).Value = x.SettingTotalAmount;  // cột 2
            ws.Cell(r, 5).Value = x.InvoiceTotalPrice;   // cột 3

            r++;
        }

        var data = ws.Range(headerRow + 1, 1, r - 1, 5);
        data.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
        data.Style.Border.InsideBorder = XLBorderStyleValues.Thin;

        ws.Columns(3, 5).Style.NumberFormat.Format = "#,##0";
        ws.Column(1).Width = 5;
        ws.Column(2).Width = 28;
        ws.Column(3).Width = 26;
        ws.Column(4).Width = 22;
        ws.Column(5).Width = 18;

        using var ms = new MemoryStream();
        wb.SaveAs(ms);

        return new ExportFeeBoardResult
        {
            FileName = $"Bang-thu-ban-tru-{request.MonthNo:D2}-{request.Year}.xlsx",
            Content = ms.ToArray()
        };
    }
}
