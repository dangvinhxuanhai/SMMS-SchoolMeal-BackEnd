using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ClosedXML.Excel;
using Microsoft.EntityFrameworkCore;
using SMMS.Application.Features.Manager.DTOs;
using SMMS.Application.Features.Manager.Interfaces;

namespace SMMS.Application.Features.Manager.Handlers;
public class ManagerFinanceService : IManagerFinanceService
{
    private readonly IManagerFinanceRepository _repo;

    public ManagerFinanceService(IManagerFinanceRepository repo)
    {
        _repo = repo;
    }
    // 🟢 6️⃣ Tìm kiếm hóa đơn theo từ khóa (học sinh, lớp, mã hóa đơn)
    public async Task<List<InvoiceDto>> SearchInvoicesAsync(Guid schoolId, string? keyword)
    {
        var query = _repo.Invoices
            .Include(i => i.Student)
            .ThenInclude(s => s.StudentClasses)
            .ThenInclude(sc => sc.Class)
            .Where(i => i.Student.StudentClasses.Any(sc => sc.Class.SchoolId == schoolId))
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(keyword))
        {
            keyword = keyword.ToLower().Trim();

            query = query.Where(i =>
                i.Student.FullName.ToLower().Contains(keyword) ||
                i.Student.StudentClasses.Any(sc => sc.Class.ClassName.ToLower().Contains(keyword)) ||
                i.InvoiceId.ToString().Contains(keyword));
        }

        var invoices = await query
            .OrderByDescending(i => i.DateFrom)
            .ToListAsync();

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
    // 🟡 7️⃣ Lọc hóa đơn theo trạng thái thanh toán
    public async Task<List<InvoiceDto>> FilterInvoicesByStatusAsync(Guid schoolId, string status)
    {
        var query = _repo.Invoices
            .Include(i => i.Student)
            .ThenInclude(s => s.StudentClasses)
            .ThenInclude(sc => sc.Class)
            .Where(i => i.Student.StudentClasses.Any(sc => sc.Class.SchoolId == schoolId))
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(status))
        {
            status = status.ToLower().Trim();
            query = query.Where(i => i.Status.ToLower() == status);
        }

        var invoices = await query
            .OrderByDescending(i => i.DateFrom)
            .ToListAsync();

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

    public async Task<FinanceSummaryDto> GetFinanceSummaryAsync(Guid schoolId, int month, int year)
    {
        // 🧾 1️⃣ Lấy hóa đơn & thanh toán trong tháng
        var invoices = await _repo.Invoices
            .Where(inv => inv.MonthNo == month)
            .Select(inv => inv.InvoiceId)
            .ToListAsync();

        var payments = await _repo.Payments
            .Where(p => invoices.Contains(p.InvoiceId))
            .ToListAsync();

        decimal totalInvoices = payments.Sum(p => p.ExpectedAmount);
        decimal totalPaid = payments.Sum(p => p.PaidAmount);
        decimal totalUnpaid = totalInvoices - totalPaid;

        // 🛒 2️⃣ Lấy chi phí đi chợ
        var purchases = await (
          from po in _repo.PurchaseOrders
          join pol in _repo.PurchaseOrderLines on po.OrderId equals pol.OrderId
          where po.SchoolId == schoolId
                && po.OrderDate.Month == month
                && po.OrderDate.Year == year
          select new
          {
              po.SupplierName,
              Amount = (pol.UnitPrice ?? 0m) * (pol.QuantityGram / 1000m)
          }
      ).ToListAsync();

        decimal totalPurchaseCost = purchases.Sum(p => p.Amount);

        var supplierBreakdown = purchases
            .GroupBy(p => p.SupplierName)
            .Select(g => new SupplierExpenseDto
            {
                Supplier = g.Key,
                Total = g.Sum(x => x.Amount)
            })
            .ToList();


        // 📊 3️⃣ Trả về DTO tổng hợp
        return new FinanceSummaryDto
        {
            SchoolId = schoolId,
            Month = month,
            Year = year,
            TotalInvoices = totalInvoices,
            PaidInvoices = totalPaid,
            UnpaidInvoices = totalUnpaid,
            TotalPurchaseCost = totalPurchaseCost,
            SupplierBreakdown = supplierBreakdown
        };
    }
    // 🟡 Danh sách hóa đơn
    // 🟡 2️⃣ Danh sách hóa đơn của trường
    public async Task<List<InvoiceDto>> GetInvoicesAsync(Guid schoolId)
    {
        var invoices = await _repo.GetInvoicesBySchoolAsync(schoolId);

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

    // 🟠 3️⃣ Chi tiết hóa đơn (gồm thông tin học sinh và thanh toán)
    public async Task<InvoiceDetailDto?> GetInvoiceDetailAsync(long invoiceId)
    {
        var inv = await _repo.GetInvoiceDetailAsync(invoiceId);
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

    // 🔵 4️⃣ Danh sách đơn hàng trong tháng
    public async Task<List<PurchaseOrderDto>> GetPurchaseOrdersByMonthAsync(Guid schoolId, int month, int year)
    {
        var orders = await _repo.PurchaseOrders
            .Include(po => po.PurchaseOrderLines)
            .Where(po => po.SchoolId == schoolId &&
                         po.OrderDate.Month == month &&
                         po.OrderDate.Year == year)
            .ToListAsync();

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

    // 🔴 5️⃣ Chi tiết đơn hàng (kèm nguyên liệu)
    public async Task<PurchaseOrderDetailDto?> GetPurchaseOrderDetailAsync(int orderId)
    {
        var order = await _repo.PurchaseOrders
            .Include(po => po.PurchaseOrderLines)
                .ThenInclude(line => line.Ingredient) // ✅ Include để lấy tên nguyên liệu
            .FirstOrDefaultAsync(po => po.OrderId == orderId);

        if (order == null)
            return null;

        // 🧮 Tổng tiền đơn hàng
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
            TotalAmount = totalAmount, // ✅ thêm tổng tiền đơn hàng
            Lines = order.PurchaseOrderLines.Select(line => new PurchaseOrderLineDto
            {
                LineId = line.LinesId,
                OrderId = line.OrderId,
                IngredientName = line.Ingredient?.IngredientName ?? "(Không rõ)", // ✅ tên nguyên liệu
                IngredientType = line.Ingredient?.IngredientType ?? "(Không rõ)",  // ✅ loại nguyên liệu (nếu cần)
                QuantityGram = line.QuantityGram / 1000m, // ✅ chuyển sang kg
                UnitPrice = line.UnitPrice ?? 0m,
                IngredientId= line.IngredientId,
                Origin = line.Origin,
                ExpiryDate = line.ExpiryDate,
                BatchNo = line.BatchNo
            }).ToList()
        };
    }

    public async Task<byte[]> ExportFinanceReportAsync(Guid schoolId, int month, int year, bool isYearly = false)
    {
        // 🧾 Lấy dữ liệu hóa đơn & thanh toán
        var invoices = await _repo.Invoices
            .Include(i => i.Student)
                .ThenInclude(s => s.StudentClasses)
                .ThenInclude(sc => sc.Class)
            .Include(i => i.Payments)
            .Where(i => i.Student.StudentClasses.Any(sc => sc.Class.SchoolId == schoolId))
            .Where(i => isYearly ? i.DateFrom.Year == year : i.MonthNo == month && i.DateFrom.Year == year)
            .ToListAsync();

        using var workbook = new XLWorkbook();
        var ws = workbook.Worksheets.Add("Báo cáo tài chính");

        // --- Header ---
        ws.Cell(1, 1).Value = "BÁO CÁO TÀI CHÍNH";
        ws.Cell(2, 1).Value = $"Thời gian: {(isYearly ? $"Năm {year}" : $"Tháng {month}/{year}")}";
        ws.Range("A1:G1").Merge().Style.Font.SetBold().Font.FontSize = 16;
        ws.Range("A2:G2").Merge().Style.Font.Italic = true;

        // --- Dòng tiêu đề ---
        ws.Cell(4, 1).Value = "Mã Hóa Đơn";
        ws.Cell(4, 2).Value = "Học Sinh";
        ws.Cell(4, 3).Value = "Lớp";
        ws.Cell(4, 4).Value = "Tháng";
        ws.Cell(4, 5).Value = "Tổng Tiền (VNĐ)";
        ws.Cell(4, 6).Value = "Đã Thanh Toán (VNĐ)";
        ws.Cell(4, 7).Value = "Trạng Thái";

        ws.Range("A4:G4").Style.Font.Bold = true;
        ws.Range("A4:G4").Style.Fill.BackgroundColor = XLColor.LightGray;

        // --- Dữ liệu ---
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

        // --- Tổng cộng ---
        ws.Cell(row + 1, 4).Value = "Tổng cộng:";
        ws.Cell(row + 1, 5).Value = totalExpected;
        ws.Cell(row + 1, 6).Value = totalPaid;

        ws.Range($"A4:G{row}").Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
        ws.Columns().AdjustToContents();

        // --- Xuất file ---
        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        return stream.ToArray();
    }

    public async Task<byte[]> ExportPurchaseReportAsync(Guid schoolId, int month, int year, bool isYearly = false)
    {
        // 🛒 Lấy danh sách đơn hàng + chi tiết nguyên liệu + thông tin nguyên liệu
        var purchaseOrders = await _repo.PurchaseOrders
            .Include(po => po.PurchaseOrderLines)
                .ThenInclude(line => line.Ingredient) // ✅ Include Ingredient để lấy tên
            .Where(po => po.SchoolId == schoolId &&
                         (isYearly
                            ? po.OrderDate.Year == year
                            : po.OrderDate.Month == month && po.OrderDate.Year == year))
            .ToListAsync();

        using var workbook = new XLWorkbook();
        var ws = workbook.Worksheets.Add("Chi phí đi chợ");

        // --- Header ---
        ws.Cell(1, 1).Value = "BÁO CÁO CHI PHÍ ĐI CHỢ";
        ws.Cell(2, 1).Value = $"Thời gian: {(isYearly ? $"Năm {year}" : $"Tháng {month}/{year}")}";
        ws.Range("A1:H1").Merge().Style.Font.SetBold().Font.FontSize = 16;
        ws.Range("A2:H2").Merge().Style.Font.Italic = true;

        // --- Dòng tiêu đề ---
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
            // 🧾 Tổng tiền đơn hàng
            decimal total = po.PurchaseOrderLines.Sum(line =>
                (line.QuantityGram / 1000m) * (line.UnitPrice ?? 0m));
            grandTotal += total;

            // --- Dòng đơn hàng ---
            ws.Cell(row, 1).Value = po.OrderDate.ToString("dd/MM/yyyy");
            ws.Cell(row, 2).Value = po.SupplierName;
            ws.Cell(row, 3).Value = po.Note;
            ws.Cell(row, 4).Value = total;
            ws.Cell(row, 5).Value = po.PurchaseOrderStatus;
            ws.Range($"A{row}:E{row}").Style.Font.SetBold();
            row++;

            // --- Header chi tiết ---
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

            row++; // dòng trống ngăn cách đơn hàng
        }

        // --- Tổng cộng ---
        ws.Cell(row + 1, 3).Value = "Tổng cộng:";
        ws.Cell(row + 1, 4).Value = grandTotal;
        ws.Cell(row + 1, 4).Style.Font.SetBold().Font.FontSize = 12;

        ws.Range($"A4:G{row}").Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
        ws.Columns().AdjustToContents();

        // --- Xuất file ---
        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        return stream.ToArray();
    }




}

