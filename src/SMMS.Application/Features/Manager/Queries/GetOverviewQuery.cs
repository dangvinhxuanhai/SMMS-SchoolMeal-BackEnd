using Microsoft.EntityFrameworkCore;
using SMMS.Application.Features.Manager.DTOs;
using SMMS.Application.Features.Manager.Interfaces;

namespace SMMS.Application.Features.Manager.Queries;

public class GetOverviewQuery
{
    private readonly IManagerRepository _repo;

    public GetOverviewQuery(IManagerRepository repo)
    {
        _repo = repo;
    }

    public async Task<ManagerOverviewDto> ExecuteAsync(Guid schoolId)
    {
        var now = DateTime.UtcNow;
        var startMonth = new DateTime(now.Year, now.Month, 1);
        var prevMonthStart = startMonth.AddMonths(-1);
        var prevMonthEnd = startMonth.AddDays(-1);

        // Lấy thống kê cơ bản
        var teacherCount = await _repo.GetTeacherCountAsync(schoolId);
        var studentCount = await _repo.GetStudentCountAsync(schoolId);
        var classCount = await _repo.GetClassCountAsync(schoolId);

        // Tính toán tài chính
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
}
