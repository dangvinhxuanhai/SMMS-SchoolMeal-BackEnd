using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SMMS.Application.Features.school.DTOs;
using SMMS.Application.Features.school.Interfaces;
using SMMS.Persistence.DbContextSite;

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

            const decimal SchoolMonthlyFee = 1200000m;

            var now = DateTime.UtcNow;
            var currentMonth = now.Month;
            var currentYear = now.Year;
            var previousMonth = now.AddMonths(-1).Month;
            var previousYear = now.AddMonths(-1).Year;

            // Số trường đăng ký trong tháng này
            var schoolsThisMonth = await _context.Schools
                .CountAsync(s => s.CreatedAt.Month == currentMonth && s.CreatedAt.Year == currentYear);

            // Số trường đăng ký trong tháng trước
            var schoolsLastMonth = await _context.Schools
                .CountAsync(s => s.CreatedAt.Month == previousMonth && s.CreatedAt.Year == previousYear);

            // Tính doanh thu
            var currentMonthRevenue = schoolsThisMonth * SchoolMonthlyFee;
            var previousMonthRevenue = schoolsLastMonth * SchoolMonthlyFee;

            // Tính tăng trưởng (%)
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
            };
        }
    }
}
