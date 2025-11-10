using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediatR;
using SMMS.Application.Features.Manager.DTOs;
using SMMS.Application.Features.Manager.Interfaces;
using SMMS.Application.Features.Manager.Queries;
using Microsoft.EntityFrameworkCore;
namespace SMMS.Application.Features.Manager.Handlers;
public class ManagerHandler :
    IRequestHandler<GetManagerOverviewQuery, ManagerOverviewDto>,
    IRequestHandler<GetRecentPurchasesQuery, List<RecentPurchaseDto>>,
    IRequestHandler<GetPurchaseOrderDetailsQuery, List<PurchaseOrderLineDto>>,
    IRequestHandler<GetRevenueQuery, RevenueSeriesDto>
{
    private readonly IManagerRepository _repo;

    public ManagerHandler(IManagerRepository repo)
    {
        _repo = repo;
    }

    // 🟢 1. Tổng quan dashboard Manager
    public async Task<ManagerOverviewDto> Handle(
        GetManagerOverviewQuery request,
        CancellationToken cancellationToken)
    {
        var now = DateTime.UtcNow;
        var startMonth = new DateTime(now.Year, now.Month, 1);
        var prevMonthStart = startMonth.AddMonths(-1);
        var prevMonthEnd = startMonth.AddDays(-1);

        var teacherCount = await _repo.GetTeacherCountAsync(request.SchoolId);
        var studentCount = await _repo.GetStudentCountAsync(request.SchoolId);
        var classCount = await _repo.GetClassCountAsync(request.SchoolId);

        var financeThisMonth = await _repo.Payments
            .Where(p => p.PaidAt >= startMonth && p.PaidAt < startMonth.AddMonths(1))
            .SumAsync(p => (decimal?)p.PaidAmount, cancellationToken) ?? 0;

        var financeLastMonth = await _repo.Payments
            .Where(p => p.PaidAt >= prevMonthStart && p.PaidAt <= prevMonthEnd)
            .SumAsync(p => (decimal?)p.PaidAmount, cancellationToken) ?? 0;

        double change = financeLastMonth == 0
            ? 100
            : (double)((financeThisMonth - financeLastMonth) / financeLastMonth) * 100;

        return new ManagerOverviewDto
        {
            TeacherCount = teacherCount,
            StudentCount = studentCount,
            ClassCount = classCount,
            FinanceThisMonth = financeThisMonth,
            FinanceLastMonth = financeLastMonth,
            FinanceChangePercent = Math.Round(change, 2)
        };
    }

    // 🟡 2. Danh sách đơn hàng gần đây
    public async Task<List<RecentPurchaseDto>> Handle(
        GetRecentPurchasesQuery request,
        CancellationToken cancellationToken)
    {
        return await _repo.PurchaseOrders
            .Where(o => o.SchoolId == request.SchoolId)
            .OrderByDescending(o => o.OrderDate)
            .Take(request.Take)
            .Select(o => new RecentPurchaseDto
            {
                OrderId = o.OrderId,
                SupplierName = o.SupplierName ?? "-",
                OrderDate = o.OrderDate,
                Status = o.PurchaseOrderStatus,
                Note = o.Note,
                TotalAmount = o.PurchaseOrderLines.Sum(l =>
                    (decimal?)l.QuantityGram / 1000 * (l.UnitPrice ?? 0)) ?? 0
            })
            .ToListAsync(cancellationToken);
    }

    // 🔵 3. Chi tiết các dòng đơn mua hàng
    public async Task<List<PurchaseOrderLineDto>> Handle(
        GetPurchaseOrderDetailsQuery request,
        CancellationToken cancellationToken)
    {
        return await _repo.PurchaseOrderLines
            .Include(l => l.Ingredient) // 🔹 để EF join bảng Ingredient
            .Where(l => l.OrderId == request.OrderId)
            .Select(l => new PurchaseOrderLineDto
            {
                LineId = l.LinesId,
                OrderId = l.OrderId,
                IngredientId = l.IngredientId,
                QuantityGram = l.QuantityGram / 1000,      // từ gram sang kg (nếu bạn dùng vậy)
                UnitPrice = l.UnitPrice,
                BatchNo = l.BatchNo,
                Origin = l.Origin,
                ExpiryDate = l.ExpiryDate,

                // 🔻 thêm 2 field này
                IngredientName = l.Ingredient != null
                    ? l.Ingredient.IngredientName
                    : string.Empty,
                IngredientType = l.Ingredient != null
                    ? l.Ingredient.IngredientType
                    : string.Empty
            })
            .ToListAsync(cancellationToken);
    }

    // 📊 4. Doanh thu theo khoảng thời gian (daily / monthly)
    public async Task<RevenueSeriesDto> Handle(
        GetRevenueQuery request,
        CancellationToken cancellationToken)
    {
        var from = request.From;
        var to = request.To;
        var granularity = request.Granularity?.ToLower() == "monthly"
            ? "monthly"
            : "daily";

        // 🔹 Lấy dữ liệu payment cần thiết vào bộ nhớ
        var data = await _repo.Payments
            .Include(p => p.Invoice)
                .ThenInclude(i => i.Student)
            .Where(p =>
                p.PaidAt >= from &&
                p.PaidAt <= to &&
                p.PaymentStatus == "paid" &&
                p.Invoice!.Student!.SchoolId == request.SchoolId)
            .Select(p => new { p.PaidAt, p.PaidAmount })
            .ToListAsync(cancellationToken);

        // 🔹 Group theo ngày / tháng
        var grouped = data
            .GroupBy(p => granularity == "monthly"
                ? new DateTime(p.PaidAt.Year, p.PaidAt.Month, 1)
                : p.PaidAt.Date)
            .Select(g => new
            {
                Date = g.Key,
                Amount = g.Sum(x => x.PaidAmount)
            })
            .OrderBy(x => x.Date)
            .ToList();

        return new RevenueSeriesDto
        {
            From = from,
            To = to,
            Granularity = granularity,
            Points = grouped.Select(d => new RevenuePointDto
            {
                Date = d.Date,
                Amount = d.Amount
            }).ToList(),
            Total = grouped.Sum(d => d.Amount)
        };
    }
}
