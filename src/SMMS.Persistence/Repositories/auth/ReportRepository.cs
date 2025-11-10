using Microsoft.EntityFrameworkCore;
using SMMS.Application.Features.auth.DTOs;
using SMMS.Application.Features.auth.Interfaces;
using SMMS.Persistence.DbContextSite;

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
    }
}
