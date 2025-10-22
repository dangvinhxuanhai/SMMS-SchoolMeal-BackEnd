using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SMMS.Application.Features.school.DTOs;
using SMMS.Application.Features.school.Interfaces;
using SMMS.Persistence.DbContextSite;

namespace SMMS.Persistence.Repositories.school
{
    public class AdminDashboardRepository : IAdminDashboardRepository
    {
        private readonly EduMealContext _context;

        public AdminDashboardRepository(EduMealContext context)
        {
            _context = context;
        }

        public async Task<DashboardOverviewDto> GetSystemOverviewAsync()
        {
            var totalSchools = await _context.Schools.CountAsync();
            var totalStudents = await _context.Students.CountAsync();

            // Doanh thu tháng này & tháng trước
            var now = DateTime.UtcNow;
            var currentMonth = now.Month;
            var previousMonth = now.AddMonths(-1).Month;

            //var currentRevenue = await _context.Invoices
            //    .Where(i => i.Status == "Paid" && i.DateFrom.Month == currentMonth)
            //    .SumAsync(i => (decimal?)i.TotalAmount ?? 0);

            //var prevRevenue = await _context.Invoices
            //    .Where(i => i.Status == "Paid" && i.DateFrom.Month == previousMonth)
            //    .SumAsync(i => (decimal?)i.TotalAmount ?? 0);

            //var revenueGrowth = prevRevenue == 0 ? 100 : ((currentRevenue - prevRevenue) / prevRevenue) * 100;

            //// Lấy hoạt động gần đây
            //var recentActivities = await _context.ActivityLogs
            //    .OrderByDescending(a => a.CreatedAt)
            //    .Take(5)
            //    .Select(a => new RecentActivityDto
            //    {
            //        Title = a.Description,
            //        Icon = a.Icon ?? "info",c
            //        CreatedAt = a.CreatedAt
            //    })
            //    .ToListAsync();

            return new DashboardOverviewDto
            {
                TotalSchools = totalSchools,
                TotalStudents = totalStudents
                //TotalRevenue = currentRevenue,
                //RevenueGrowth = Math.Round(revenueGrowth, 2),
            };
        }
    }
}
