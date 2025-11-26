using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SMMS.Application.Features.school.DTOs;
using SMMS.Application.Features.school.Interfaces;
using SMMS.Persistence.Data;

namespace SMMS.Persistence.Repositories.schools
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

            var now = DateTime.UtcNow;

            var currentMonth = now.Month;
            var currentYear = now.Year;

            var previous = now.AddMonths(-1);
            var previousMonth = previous.Month;
            var previousYear = previous.Year;

            // --- Lấy doanh thu từ bảng SchoolRevenues ---
            var currentMonthRevenue = await _context.SchoolRevenues
                .Where(r => r.RevenueDate.Month == currentMonth &&
                            r.RevenueDate.Year == currentYear &&
                            r.IsActive == true)
                .SumAsync(r => (decimal?)r.RevenueAmount) ?? 0;

            var previousMonthRevenue = await _context.SchoolRevenues
                .Where(r => r.RevenueDate.Month == previousMonth &&
                            r.RevenueDate.Year == previousYear &&
                            r.IsActive == true)
                .SumAsync(r => (decimal?)r.RevenueAmount) ?? 0;

            // --- Tính % tăng trưởng doanh thu ---
            decimal revenueGrowth = 0;
            if (previousMonthRevenue > 0)
            {
                revenueGrowth = ((currentMonthRevenue - previousMonthRevenue) / previousMonthRevenue) * 100;
            }

            return new DashboardOverviewDto
            {
                TotalSchools = totalSchools,
                TotalStudents = totalStudents,
                CurrentMonthRevenue = currentMonthRevenue,
                PreviousMonthRevenue = previousMonthRevenue,
                RevenueGrowth = revenueGrowth
            };
        }
    }
}
