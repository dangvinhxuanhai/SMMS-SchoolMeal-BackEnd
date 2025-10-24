using Microsoft.EntityFrameworkCore;
using SMMS.Application.Features.Manager.DTOs;
using SMMS.Application.Features.Manager.Interfaces;


namespace SMMS.Application.Features.Manager.Handlers
{
    public class ManagerService : IManagerService
    {
        private readonly IManagerRepository _repo;

        public ManagerService(IManagerRepository repo)
        {
            _repo = repo;
        }

        public async Task<ManagerOverviewDto> GetOverviewAsync(Guid schoolId)
        {
            var now = DateTime.UtcNow;
            var startMonth = new DateTime(now.Year, now.Month, 1);
            var prevMonthStart = startMonth.AddMonths(-1);
            var prevMonthEnd = startMonth.AddDays(-1);

            var teacherCount = await _repo.GetTeacherCountAsync(schoolId);
            var studentCount = await _repo.GetStudentCountAsync(schoolId);
            var classCount = await _repo.GetClassCountAsync(schoolId);

            var financeThisMonth = await _repo.Payments
                .Where(p => p.PaidAt >= startMonth && p.PaidAt < startMonth.AddMonths(1))
                .SumAsync(p => (decimal?)p.PaidAmount) ?? 0;

            var financeLastMonth = await _repo.Payments
                .Where(p => p.PaidAt >= prevMonthStart && p.PaidAt <= prevMonthEnd)
                .SumAsync(p => (decimal?)p.PaidAmount) ?? 0;

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

        public async Task<List<RecentPurchaseDto>> GetRecentPurchasesAsync(Guid schoolId, int take = 8)
        {
            return await _repo.PurchaseOrders
                .Where(o => o.SchoolId == schoolId)
                .OrderByDescending(o => o.OrderDate)
                .Take(take)
                .Select(o => new RecentPurchaseDto
                {
                    OrderId = o.OrderId,
                    SupplierName = o.SupplierName ?? "-",
                    OrderDate = o.OrderDate,
                    Status = o.PurchaseOrderStatus,
                    Note = o.Note,
                    TotalAmount = o.PurchaseOrderLines.Sum(l =>
                        (decimal?)l.QuantityGram * (l.UnitPrice ?? 0)) ?? 0
                })
                .ToListAsync();
        }
        // 🔵 Lấy chi tiết đơn mua hàng
        public async Task<List<PurchaseOrderLineDto>> GetPurchaseOrderDetailsAsync(int orderId)
        {
            return await _repo.PurchaseOrderLines
                .Where(l => l.OrderId == orderId)
                .Select(l => new PurchaseOrderLineDto
                {
                    LinesId = l.LinesId,
                    OrderId = l.OrderId,
                    IngredientId = l.IngredientId,
                    QuantityGram = l.QuantityGram,
                    UnitPrice = l.UnitPrice,
                    BatchNo = l.BatchNo,
                    Origin = l.Origin,
                    ExpiryDate = l.ExpiryDate
                })
                .ToListAsync();
        }
        public async Task<RevenueSeriesDto> GetRevenueAsync(Guid schoolId, DateTime from, DateTime to, string granularity = "daily")
        {
            // 🔹 Lấy tất cả payment hợp lệ, include invoice + student
            var query = _repo.Payments
                .Include(p => p.Invoice)
                    .ThenInclude(i => i.Student)
                .Where(p =>
                    p.PaidAt >= from &&
                    p.PaidAt <= to &&
                    p.PaymentStatus == "paid" &&
                    p.Invoice!.Student!.SchoolId == schoolId);

            // 🔹 Lấy dữ liệu vào bộ nhớ để tính group (EF không translate .Date)
            var data = await _repo.Payments
     .Include(p => p.Invoice).ThenInclude(i => i.Student)
     .Where(p =>
         p.PaidAt >= from &&
         p.PaidAt <= to &&
         p.PaymentStatus == "paid" &&
         p.Invoice!.Student!.SchoolId == schoolId)
     .Select(p => new { p.PaidAt, p.PaidAmount })
     .ToListAsync(); // tải dữ liệu cần thiết trước

            var grouped = data
                .GroupBy(p => granularity == "monthly"
                    ? new DateTime(p.PaidAt.Year, p.PaidAt.Month, 1)
                    : p.PaidAt.Date)
                .Select(g => new { Date = g.Key, Amount = g.Sum(x => x.PaidAmount) })
                .OrderBy(x => x.Date)
                .ToList();

            // 🔹 Tạo DTO trả về
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
}
