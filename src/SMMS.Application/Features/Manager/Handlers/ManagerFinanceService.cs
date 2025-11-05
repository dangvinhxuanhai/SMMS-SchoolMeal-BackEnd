using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
            .FirstOrDefaultAsync(po => po.OrderId == orderId);

        if (order == null)
            return null;

        return new PurchaseOrderDetailDto
        {
            OrderId = order.OrderId,
            SchoolId = order.SchoolId,
            OrderDate = order.OrderDate,
            SupplierName = order.SupplierName,
            PurchaseOrderStatus = order.PurchaseOrderStatus,
            Note = order.Note,
            Lines = order.PurchaseOrderLines.Select(line => new PurchaseOrderLineDto
            {
                LineId = line.LinesId,
                BatchNo = line.BatchNo,
                QuantityGram = line.QuantityGram/1000,
                UnitPrice = line.UnitPrice ?? 0m,
                Origin = line.Origin,
                ExpiryDate = line.ExpiryDate
            }).ToList()
        };
    }
}

