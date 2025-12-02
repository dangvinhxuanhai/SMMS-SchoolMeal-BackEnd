using Microsoft.EntityFrameworkCore;
using SMMS.Application.Features.auth.DTOs;
using SMMS.Application.Features.auth.Interfaces;
using SMMS.Persistence.Data;

namespace SMMS.Infrastructure.Repositories.Implementations
{
    public class ReportRepository : IReportRepository
    {
        private readonly EduMealContext _context;

        public ReportRepository(EduMealContext context)
        {
            _context = context;
        }

        public async Task<List<UserReportDto>> GetUserReportAsync(ReportFilterDto filter)
        {
            var query = _context.Users
                .Include(u => u.Role)
                .Include(u => u.School)
                .AsQueryable();

            // Nếu lọc theo trường
            if (filter.Scope == "TheoTruong" && filter.SchoolId.HasValue)
            {
                query = query.Where(u => u.SchoolId == filter.SchoolId);
            }

            // Nếu lọc theo thời gian
            if (filter.FromDate.HasValue && filter.ToDate.HasValue)
            {
                query = query.Where(u => u.CreatedAt >= filter.FromDate && u.CreatedAt <= filter.ToDate);
            }

            // Gom nhóm thống kê
            var result = await query
                .GroupBy(u => new
                {
                    u.Role.RoleName,
                    SchoolName = u.School != null ? u.School.SchoolName : "Hệ thống"
                })
                .Select(g => new UserReportDto
                {
                    RoleName = g.Key.RoleName,
                    SchoolName = g.Key.SchoolName,
                    TotalUsers = g.Count(),
                    ActiveUsers = g.Count(u => u.IsActive),
                    InactiveUsers = g.Count(u => !u.IsActive)
                })
                .ToListAsync();

            return result;
        }
        public async Task<List<UserReportDto>> GetAllUserReportAsync()
        {
            var result = await _context.Users
                .Include(u => u.Role)
                .Include(u => u.School)
                .GroupBy(u => new
                {
                    u.Role.RoleName,
                    SchoolName = u.School != null ? u.School.SchoolName : "Hệ thống"
                })
                .Select(g => new UserReportDto
                {
                    RoleName = g.Key.RoleName,
                    SchoolName = g.Key.SchoolName,
                    TotalUsers = g.Count(),
                    ActiveUsers = g.Count(u => u.IsActive),
                    InactiveUsers = g.Count(u => !u.IsActive)
                })
                .ToListAsync();

            return result;
        }

        public async Task<List<FinanceReportDto>> GetFinanceReportAsync(FinanceReportFilterDto filter)
        {
            var query = _context.SchoolRevenues
                .Include(r => r.School)
                .Where(r => r.IsActive)
                .AsQueryable();

            // Lọc theo trường
            if (filter.Scope == "TheoTruong" && filter.SchoolId.HasValue)
            {
                query = query.Where(r => r.SchoolId == filter.SchoolId);
            }

            // Lọc theo thời gian
            if (filter.FromDate.HasValue && filter.ToDate.HasValue)
            {
                query = query
                    .Where(r => r.RevenueDate >= filter.FromDate &&
                                r.RevenueDate <= filter.ToDate);
            }

            // Gom nhóm báo cáo
            var result = await query
                .GroupBy(r => new
                {
                    SchoolName = r.School != null ? r.School.SchoolName : "Chưa gán trường"
                })
                .Select(g => new FinanceReportDto
                {
                    SchoolName = g.Key.SchoolName,
                    RevenueCount = g.Count(),
                    TotalRevenue = g.Sum(x => x.RevenueAmount),
                })
                .ToListAsync();

            return result;
        }
        public async Task<List<FinanceReportDto>> GetAllFinanceReportAsync()
        {
            var result = await _context.SchoolRevenues
                .Include(r => r.School)
                .Where(r => r.IsActive)
                .GroupBy(r => new
                {
                    SchoolName = r.School != null ? r.School.SchoolName : "Chưa gán trường"
                })
                .Select(g => new FinanceReportDto
                {
                    SchoolName = g.Key.SchoolName,
                    RevenueCount = g.Count(),
                    TotalRevenue = g.Sum(x => x.RevenueAmount)
                })
                .ToListAsync();

            return result;
        }

    }
}
