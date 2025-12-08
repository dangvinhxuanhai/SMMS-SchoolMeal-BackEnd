using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
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
        CancellationToken cancellationToken)
    {
        var query =
            from i in _repo.Invoices
            join s in _repo.Students on i.StudentId equals s.StudentId
            where s.SchoolId == request.SchoolId
            select new { i, s };

        if (request.MonthNo.HasValue)
            query = query.Where(x => x.i.MonthNo == request.MonthNo);

        if (request.Year.HasValue)
            query = query.Where(x => x.i.DateFrom.Year == request.Year.Value);

        if (!string.IsNullOrWhiteSpace(request.Status))
            query = query.Where(x => x.i.Status == request.Status);

        return await query
         .Select(x => new InvoiceDto1
         {
             InvoiceId = x.i.InvoiceId,
             InvoiceCode = x.i.InvoiceCode,
             StudentId = x.i.StudentId,
             StudentName = x.s.FullName, // 👈 map tên học sinh
             MonthNo = x.i.MonthNo,
             DateFrom = x.i.DateFrom.ToDateTime(TimeOnly.MinValue),
             DateTo = x.i.DateTo.ToDateTime(TimeOnly.MinValue),
             AbsentDay = x.i.AbsentDay,
             Status = x.i.Status
         })
         .ToListAsync(cancellationToken);
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
        var invoice =
            await (from i in _repo.Invoices
                   join s in _repo.Students on i.StudentId equals s.StudentId
                   where i.InvoiceId == request.InvoiceId &&
                         s.SchoolId == request.SchoolId
                   select i)
            .FirstOrDefaultAsync(cancellationToken);

        if (invoice == null)
            return null;

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
public class GenerateSchoolInvoicesHandler :
    IRequestHandler<GenerateSchoolInvoicesCommand, IReadOnlyList<InvoiceDto1>>
{
    private readonly ISchoolInvoiceRepository _repo;
    private readonly IManagerPaymentSettingRepository _paymentRepo;

    public GenerateSchoolInvoicesHandler(
        ISchoolInvoiceRepository repo,
        IManagerPaymentSettingRepository paymentRepo)
    {
        _repo = repo;
        _paymentRepo = paymentRepo;
    }

    public async Task<IReadOnlyList<InvoiceDto1>> Handle(
        GenerateSchoolInvoicesCommand request,
        CancellationToken ct)
    {
        var dtFrom = request.Request.DateFrom.Date;
        var dtTo = request.Request.DateTo.Date;

        // 0️⃣ Validate cơ bản
        if (dtFrom > dtTo)
            throw new ArgumentException("DateFrom must be <= DateTo.");

        if (dtFrom.Year != dtTo.Year)
            throw new ArgumentException("Không được tạo invoice cho nhiều năm khác nhau.");

        short fromMonth = (short)dtFrom.Month;
        short toMonth = (short)dtTo.Month;

        if (fromMonth < 1 || fromMonth > 12 ||
            toMonth < 1 || toMonth > 12)
        {
            throw new ArgumentException("Tháng phải nằm trong khoảng từ 1 đến 12.");
        }

        if (fromMonth > toMonth)
            throw new ArgumentException("Tháng bắt đầu không được lớn hơn tháng kết thúc.");

        // 1️⃣ BẮT BUỘC phải trùng đúng 1 cấu hình thu phí
        var setting = await _paymentRepo.GetExactRangeAsync(
            request.SchoolId,
            fromMonth,
            toMonth,
            ct);

        if (setting == null)
        {
            // ❌ Không có cấu hình (fromMonth, toMonth) tương ứng
            throw new InvalidOperationException(
                "Nhập ngày không đúng trong cấu hình cài đặt thanh toán.");
        }

        // MonthNo của invoice: lấy theo DateFrom (tức fromMonth)
        short monthNo = fromMonth;

        var fromD = DateOnly.FromDateTime(dtFrom);
        var toD = DateOnly.FromDateTime(dtTo);

        // 2️⃣ Lấy học sinh active của trường
        var students = await _repo.Students
            .Where(s => s.SchoolId == request.SchoolId && s.IsActive)
            .Select(s => s.StudentId)
            .ToListAsync(ct);

        if (!students.Any())
            return Array.Empty<InvoiceDto1>();

        // 3️⃣ Tính absent trong khoảng ngày
        var absentMap = await _repo.Attendance
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

        // 4️⃣ Invoice đã tồn tại (overlap khoảng ngày)
        var existedStudentIds = await _repo.Invoices
            .Where(i =>
                students.Contains(i.StudentId) &&
                i.DateFrom <= toD &&
                i.DateTo >= fromD)
            .Select(i => i.StudentId)
            .Distinct()
            .ToListAsync(ct);

        var existed = existedStudentIds.ToHashSet();

        // 5️⃣ Tạo invoice mới
        var newInvoices = new List<Invoice>();

        foreach (var sid in students)
        {
            if (request.Request.SkipExisting && existed.Contains(sid))
                continue;

            var absent = absentMap.TryGetValue(sid, out var c) ? c : 0;

            newInvoices.Add(new Invoice
            {
                InvoiceCode = Guid.NewGuid(),
                StudentId = sid,
                MonthNo = monthNo,
                DateFrom = fromD,
                DateTo = toD,
                AbsentDay = absent,
                Status = "Unpaid"
            });
        }

        if (newInvoices.Any())
            await _repo.AddInvoicesAsync(newInvoices, ct);

        await _repo.SaveChangesAsync(ct);

        return newInvoices
            .Select(i => new InvoiceDto1
            {
                InvoiceId = i.InvoiceId,
                InvoiceCode = i.InvoiceCode,
                StudentId = i.StudentId,
                MonthNo = i.MonthNo,
                DateFrom = i.DateFrom.ToDateTime(TimeOnly.MinValue),
                DateTo = i.DateTo.ToDateTime(TimeOnly.MinValue),
                AbsentDay = i.AbsentDay,
                Status = i.Status
            })
            .ToList();
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
