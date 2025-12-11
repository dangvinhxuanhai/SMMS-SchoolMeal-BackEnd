using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SMMS.Application.Features.Manager.Handlers;

using ClosedXML.Excel;
using MediatR;
using Microsoft.EntityFrameworkCore;
using SMMS.Application.Features.Manager.DTOs;
using SMMS.Application.Features.Manager.Interfaces;
using SMMS.Application.Features.Manager.Queries;

public class ManagerFinanceHandler :
    IRequestHandler<SearchInvoicesQuery, List<InvoiceDto>>,
    IRequestHandler<FilterInvoicesByStatusQuery, List<InvoiceDto>>,
    IRequestHandler<GetFinanceSummaryQuery, FinanceSummaryDto>,
    IRequestHandler<GetInvoicesQuery, List<InvoiceDto>>,
    IRequestHandler<GetInvoiceDetailQuery, InvoiceDetailDto?>,
    IRequestHandler<GetPurchaseOrdersByMonthQuery, List<PurchaseOrderDto>>,
    IRequestHandler<GetPurchaseOrderDetailQuery, PurchaseOrderDetailDto?>,
    IRequestHandler<ExportFinanceReportQuery, byte[]>,
    IRequestHandler<ExportPurchaseReportQuery, byte[]>
{
    private readonly IManagerFinanceRepository _repo;

    public ManagerFinanceHandler(IManagerFinanceRepository repo)
    {
        _repo = repo;
    }

    #region 1️⃣ SearchInvoicesAsync

    public async Task<List<InvoiceDto>> Handle(
        SearchInvoicesQuery request,
        CancellationToken cancellationToken)
    {
        var query = _repo.Invoices
            .Include(i => i.Student)
            .ThenInclude(s => s.StudentClasses)
            .ThenInclude(sc => sc.Class)
            .Where(i => i.Student.StudentClasses.Any(sc => sc.Class.SchoolId == request.SchoolId))
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(request.Keyword))
        {
            var keyword = request.Keyword.ToLower().Trim();

            query = query.Where(i =>
                i.Student.FullName.ToLower().Contains(keyword) ||
                i.Student.StudentClasses.Any(sc => sc.Class.ClassName.ToLower().Contains(keyword)) ||
                i.InvoiceId.ToString().Contains(keyword));
        }

        var invoices = await query
            .OrderByDescending(i => i.DateFrom)
            .ToListAsync(cancellationToken);

        return invoices.Select(inv => new InvoiceDto
        {
            InvoiceId = inv.InvoiceId,
            StudentName = inv.Student.FullName,
            ClassName = inv.Student.StudentClasses
                .Select(sc => sc.Class.ClassName)
                .FirstOrDefault() ?? "(Chưa có lớp)",
            MonthNo = inv.MonthNo,
            DateFrom = inv.DateFrom.ToDateTime(TimeOnly.MinValue),
            DateTo = inv.DateTo.ToDateTime(TimeOnly.MinValue),
            AbsentDay = inv.AbsentDay,
            Status = inv.Status
        }).ToList();
    }

    #endregion

    #region 2️⃣ FilterInvoicesByStatusAsync

    public async Task<List<InvoiceDto>> Handle(
        FilterInvoicesByStatusQuery request,
        CancellationToken cancellationToken)
    {
        var query = _repo.Invoices
            .Include(i => i.Student)
            .ThenInclude(s => s.StudentClasses)
            .ThenInclude(sc => sc.Class)
            .Where(i => i.Student.StudentClasses.Any(sc => sc.Class.SchoolId == request.SchoolId))
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(request.Status) && request.Status != "all")
        {
            var status = request.Status.ToLower().Trim();
            var today = DateOnly.FromDateTime(DateTime.Now);
            switch (status)
            {
                case "paid":
                    // ✅ Đã thanh toán
                    query = query.Where(i => i.Status == "Paid");
                    break;

                case "overdue":
                    query = query.Where(i => (i.Status == "Unpaid" || i.Status == "Pending") && i.DateTo < today);
                    break;

                case "pending":
                    query = query.Where(i => (i.Status == "Unpaid" || i.Status == "Pending") && i.DateTo >= today);
                    break;

                default:
                    query = query.Where(i => i.Status.ToLower() == status);
                    break;
            }
        }

        var invoices = await query
            .OrderByDescending(i => i.DateFrom)
            .ToListAsync(cancellationToken);

        return invoices.Select(inv => new InvoiceDto
        {
            InvoiceId = inv.InvoiceId,
            StudentName = inv.Student.FullName,
            ClassName = inv.Student.StudentClasses
                .Select(sc => sc.Class.ClassName)
                .FirstOrDefault() ?? "(Chưa có lớp)",
            MonthNo = inv.MonthNo,
            DateFrom = inv.DateFrom.ToDateTime(TimeOnly.MinValue),
            DateTo = inv.DateTo.ToDateTime(TimeOnly.MinValue),
            AbsentDay = inv.AbsentDay,
            Status = GetDisplayStatus(inv.Status, inv.DateTo)
        }).ToList();
    }

    private string GetDisplayStatus(string dbStatus, DateOnly dateTo)
    {
        if (dbStatus == "Paid") return "Paid";
        if (dateTo < DateOnly.FromDateTime(DateTime.Now)) return "Overdue";
        return "Pending";
    }

    #endregion

    #region 3️⃣ GetFinanceSummaryAsync (theo năm)

    public async Task<FinanceSummaryDto> Handle(
        GetFinanceSummaryQuery request,
        CancellationToken cancellationToken)
    {
        var schoolId = request.SchoolId;
        var year = request.Year;

        // 1️⃣ Lấy hóa đơn & thanh toán trong NĂM (theo DateFrom.Year)
        var invoiceIds = await _repo.Invoices
            .Where(inv => inv.DateFrom.Year == year)
            .Select(inv => inv.InvoiceId)
            .ToListAsync(cancellationToken);

        var payments = await _repo.Payments
            .Where(p => invoiceIds.Contains(p.InvoiceId))
            .ToListAsync(cancellationToken);

        // Tổng doanh thu dự kiến trong năm
        decimal totalInvoices = payments.Sum(p => p.ExpectedAmount);

        // 2️⃣ Lấy chi phí đi chợ trong NĂM
        var purchases = await (
            from po in _repo.PurchaseOrders
            join pol in _repo.PurchaseOrderLines on po.OrderId equals pol.OrderId
            where po.SchoolId == schoolId
                  && po.OrderDate.Year == year
            select new
            {
                po.SupplierName,
                Amount = (pol.UnitPrice ?? 0m) * (pol.QuantityGram / 1000m)
            }
        ).ToListAsync(cancellationToken);

        decimal totalPurchaseCost = purchases.Sum(p => p.Amount);

        var supplierBreakdown = purchases
            .GroupBy(p => p.SupplierName)
            .Select(g => new SupplierExpenseDto
            {
                Supplier = g.Key,
                Total = g.Sum(x => x.Amount)
            })
            .ToList();

        // 3️⃣ Trả về summary theo NĂM
        return new FinanceSummaryDto
        {
            SchoolId = schoolId,
            Year = year,
            TotalInvoices = totalInvoices,
            TotalPurchaseCost = totalPurchaseCost,
            SupplierBreakdown = supplierBreakdown
        };
    }

    #endregion


    #region 4️⃣ GetInvoicesAsync

    public async Task<List<InvoiceDto>> Handle(
        GetInvoicesQuery request,
        CancellationToken cancellationToken)
    {
        var invoices = await _repo.GetInvoicesBySchoolAsync(request.SchoolId);

        return invoices.Select(inv => new InvoiceDto
        {
            InvoiceId = inv.InvoiceId,
            StudentName = inv.Student.FullName,
            ClassName = inv.Student.StudentClasses
                .Select(sc => sc.Class.ClassName)
                .FirstOrDefault() ?? "(Chưa có lớp)",
            MonthNo = inv.MonthNo,
            DateFrom = inv.DateFrom.ToDateTime(TimeOnly.MinValue),
            DateTo = inv.DateTo.ToDateTime(TimeOnly.MinValue),
            AbsentDay = inv.AbsentDay,
            Status = inv.Status
        }).ToList();
    }

    #endregion

    #region 5️⃣ GetInvoiceDetailAsync

    public async Task<InvoiceDetailDto?> Handle(
        GetInvoiceDetailQuery request,
        CancellationToken cancellationToken)
    {
        var inv = await _repo.GetInvoiceDetailAsync(request.InvoiceId);
        if (inv == null) return null;

        return new InvoiceDetailDto
        {
            InvoiceId = inv.InvoiceId,
            StudentName = inv.Student.FullName,
            ClassName = inv.Student.StudentClasses
                .Select(sc => sc.Class.ClassName)
                .FirstOrDefault() ?? "(Chưa có lớp)",
            MonthNo = inv.MonthNo,
            DateFrom = inv.DateFrom.ToDateTime(TimeOnly.MinValue),
            DateTo = inv.DateTo.ToDateTime(TimeOnly.MinValue),
            Status = inv.Status,
            Payments = inv.Payments.Select(p => new PaymentDto
            {
                PaymentId = p.PaymentId,
                ExpectedAmount = p.ExpectedAmount,
                PaidAmount = p.PaidAmount,
                PaymentStatus = p.PaymentStatus,
                Method = p.Method,
                PaidAt = p.PaidAt
            }).ToList()
        };
    }

    #endregion

    #region 6️⃣ GetPurchaseOrdersByMonthAsync

    public async Task<List<PurchaseOrderDto>> Handle(
        GetPurchaseOrdersByMonthQuery request,
        CancellationToken cancellationToken)
    {
        var orders = await _repo.PurchaseOrders
            .Include(po => po.PurchaseOrderLines)
            .Where(po => po.SchoolId == request.SchoolId &&
                         po.OrderDate.Month == request.Month &&
                         po.OrderDate.Year == request.Year)
            .ToListAsync(cancellationToken);

        return orders.Select(po => new PurchaseOrderDto
        {
            OrderId = po.OrderId,
            SchoolId = po.SchoolId,
            OrderDate = po.OrderDate,
            SupplierName = po.SupplierName,
            PurchaseOrderStatus = po.PurchaseOrderStatus,
            Note = po.Note,
            TotalAmount = po.PurchaseOrderLines.Sum(line =>
                (line.QuantityGram / 1000m) * (line.UnitPrice ?? 0m))
        }).ToList();
    }

    #endregion

    #region 7️⃣ GetPurchaseOrderDetailAsync

    public async Task<PurchaseOrderDetailDto?> Handle(
        GetPurchaseOrderDetailQuery request,
        CancellationToken cancellationToken)
    {
        var order = await _repo.PurchaseOrders
            .Include(po => po.PurchaseOrderLines)
            .ThenInclude(line => line.Ingredient)
            .FirstOrDefaultAsync(po => po.OrderId == request.OrderId, cancellationToken);

        if (order == null)
            return null;

        decimal totalAmount = order.PurchaseOrderLines.Sum(line =>
            (line.QuantityGram / 1000m) * (line.UnitPrice ?? 0m));

        return new PurchaseOrderDetailDto
        {
            OrderId = order.OrderId,
            SchoolId = order.SchoolId,
            OrderDate = order.OrderDate,
            SupplierName = order.SupplierName,
            PurchaseOrderStatus = order.PurchaseOrderStatus,
            Note = order.Note,
            TotalAmount = totalAmount,
            Lines = order.PurchaseOrderLines.Select(line => new PurchaseOrderLineDto
            {
                LineId = line.LinesId,
                OrderId = line.OrderId,
                IngredientName = line.Ingredient?.IngredientName ?? "(Không rõ)",
                IngredientType = line.Ingredient?.IngredientType ?? "(Không rõ)",
                QuantityGram = line.QuantityGram / 1000m,
                UnitPrice = line.UnitPrice ?? 0m,
                IngredientId = line.IngredientId,
                Origin = line.Origin,
                ExpiryDate = line.ExpiryDate,
                BatchNo = line.BatchNo
            }).ToList()
        };
    }

    #endregion

    #region 8️⃣ ExportFinanceReportAsync

    public async Task<byte[]> Handle(
        ExportFinanceReportQuery request,
        CancellationToken cancellationToken)
    {
        var (schoolId, month, year, isYearly) =
            (request.SchoolId, request.Month, request.Year, request.IsYearly);

        var invoices = await _repo.Invoices
            .Include(i => i.Student)
            .ThenInclude(s => s.StudentClasses)
            .ThenInclude(sc => sc.Class)
            .Include(i => i.Payments)
            .Where(i => i.Student.StudentClasses.Any(sc => sc.Class.SchoolId == schoolId))
            .Where(i => isYearly
                ? i.DateFrom.Year == year
                : i.MonthNo == month && i.DateFrom.Year == year)
            .ToListAsync(cancellationToken);

        using var workbook = new XLWorkbook();
        var ws = workbook.Worksheets.Add("Báo cáo tài chính");

        ws.Cell(1, 1).Value = "BÁO CÁO TÀI CHÍNH";
        ws.Cell(2, 1).Value = $"Thời gian: {(isYearly ? $"Năm {year}" : $"Tháng {month}/{year}")}";
        ws.Range("A1:G1").Merge().Style.Font.SetBold().Font.FontSize = 16;
        ws.Range("A2:G2").Merge().Style.Font.Italic = true;

        ws.Cell(4, 1).Value = "Mã Hóa Đơn";
        ws.Cell(4, 2).Value = "Học Sinh";
        ws.Cell(4, 3).Value = "Lớp";
        ws.Cell(4, 4).Value = "Tháng";
        ws.Cell(4, 5).Value = "Tổng Tiền (VNĐ)";
        ws.Cell(4, 6).Value = "Đã Thanh Toán (VNĐ)";
        ws.Cell(4, 7).Value = "Trạng Thái";

        ws.Range("A4:G4").Style.Font.Bold = true;
        ws.Range("A4:G4").Style.Fill.BackgroundColor = XLColor.LightGray;

        int row = 5;
        decimal totalExpected = 0, totalPaid = 0;

        foreach (var inv in invoices)
        {
            decimal expected = inv.Payments.Sum(p => p.ExpectedAmount);
            decimal paid = inv.Payments.Sum(p => p.PaidAmount);

            totalExpected += expected;
            totalPaid += paid;

            ws.Cell(row, 1).Value = inv.InvoiceId;
            ws.Cell(row, 2).Value = inv.Student.FullName;
            ws.Cell(row, 3).Value = inv.Student.StudentClasses
                .Select(sc => sc.Class.ClassName)
                .FirstOrDefault() ?? "(Chưa có lớp)";
            ws.Cell(row, 4).Value = inv.MonthNo;
            ws.Cell(row, 5).Value = expected;
            ws.Cell(row, 6).Value = paid;
            ws.Cell(row, 7).Value = inv.Status;

            row++;
        }

        ws.Cell(row + 1, 4).Value = "Tổng cộng:";
        ws.Cell(row + 1, 5).Value = totalExpected;
        ws.Cell(row + 1, 6).Value = totalPaid;

        ws.Range($"A4:G{row}").Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
        ws.Columns().AdjustToContents();

        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        return stream.ToArray();
    }

    #endregion

    #region 9️⃣ ExportPurchaseReportAsync

    public async Task<byte[]> Handle(
        ExportPurchaseReportQuery request,
        CancellationToken cancellationToken)
    {
        var (schoolId, month, year, isYearly) =
            (request.SchoolId, request.Month, request.Year, request.IsYearly);

        var purchaseOrders = await _repo.PurchaseOrders
            .Include(po => po.PurchaseOrderLines)
            .ThenInclude(line => line.Ingredient)
            .Where(po => po.SchoolId == schoolId &&
                         (isYearly
                             ? po.OrderDate.Year == year
                             : po.OrderDate.Month == month && po.OrderDate.Year == year))
            .ToListAsync(cancellationToken);

        using var workbook = new XLWorkbook();
        var ws = workbook.Worksheets.Add("Chi phí đi chợ");

        ws.Cell(1, 1).Value = "BÁO CÁO CHI PHÍ ĐI CHỢ";
        ws.Cell(2, 1).Value = $"Thời gian: {(isYearly ? $"Năm {year}" : $"Tháng {month}/{year}")}";
        ws.Range("A1:H1").Merge().Style.Font.SetBold().Font.FontSize = 16;
        ws.Range("A2:H2").Merge().Style.Font.Italic = true;

        ws.Cell(4, 1).Value = "Ngày Mua";
        ws.Cell(4, 2).Value = "Nhà Cung Cấp";
        ws.Cell(4, 3).Value = "Ghi Chú";
        ws.Cell(4, 4).Value = "Tổng Tiền (VNĐ)";
        ws.Cell(4, 5).Value = "Trạng Thái";

        ws.Range("A4:E4").Style.Font.Bold = true;
        ws.Range("A4:E4").Style.Fill.BackgroundColor = XLColor.LightGray;

        int row = 5;
        decimal grandTotal = 0;

        foreach (var po in purchaseOrders)
        {
            decimal total = po.PurchaseOrderLines.Sum(line =>
                (line.QuantityGram / 1000m) * (line.UnitPrice ?? 0m));
            grandTotal += total;

            ws.Cell(row, 1).Value = po.OrderDate.ToString("dd/MM/yyyy");
            ws.Cell(row, 2).Value = po.SupplierName;
            ws.Cell(row, 3).Value = po.Note;
            ws.Cell(row, 4).Value = total;
            ws.Cell(row, 5).Value = po.PurchaseOrderStatus;
            ws.Range($"A{row}:E{row}").Style.Font.SetBold();
            row++;

            ws.Cell(row, 2).Value = "Nguyên liệu";
            ws.Cell(row, 3).Value = "Số lượng (kg)";
            ws.Cell(row, 4).Value = "Đơn giá (VNĐ/kg)";
            ws.Cell(row, 5).Value = "Thành tiền (VNĐ)";
            ws.Cell(row, 6).Value = "Nguồn gốc";
            ws.Cell(row, 7).Value = "Hạn sử dụng";

            ws.Range($"B{row}:G{row}").Style.Font.Bold = true;
            ws.Range($"B{row}:G{row}").Style.Fill.BackgroundColor = XLColor.LightGray;
            row++;

            foreach (var line in po.PurchaseOrderLines)
            {
                decimal lineTotal = (line.QuantityGram / 1000m) * (line.UnitPrice ?? 0m);

                ws.Cell(row, 2).Value = line.Ingredient?.IngredientName ?? "(Không rõ)";
                ws.Cell(row, 3).Value = line.QuantityGram / 1000m;
                ws.Cell(row, 4).Value = line.UnitPrice ?? 0m;
                ws.Cell(row, 5).Value = lineTotal;
                ws.Cell(row, 6).Value = line.Origin;
                ws.Cell(row, 7).Value = line.ExpiryDate?.ToString("dd/MM/yyyy") ?? "";

                row++;
            }

            row++;
        }

        ws.Cell(row + 1, 3).Value = "Tổng cộng:";
        ws.Cell(row + 1, 4).Value = grandTotal;
        ws.Cell(row + 1, 4).Style.Font.SetBold().Font.FontSize = 12;

        ws.Range($"A4:G{row}").Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
        ws.Columns().AdjustToContents();

        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        return stream.ToArray();
    }

    #endregion
}
