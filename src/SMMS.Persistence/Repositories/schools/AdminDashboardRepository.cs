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

            // Doanh thu tháng này & tháng trước
            var now = DateTime.UtcNow;
            var currentMonth = now.Month;
            var previousMonth = now.AddMonths(-1).Month;

            return new DashboardOverviewDto
            {
                TotalSchools = totalSchools,
                TotalStudents = totalStudents
            };
        }
    }
}
