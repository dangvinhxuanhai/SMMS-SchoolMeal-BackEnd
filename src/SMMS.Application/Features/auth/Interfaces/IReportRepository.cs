using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SMMS.Application.Features.auth.DTOs;

namespace SMMS.Application.Features.auth.Interfaces
{
    public interface IReportRepository
    {
        Task<List<UserReportDto>> GetUserReportAsync(ReportFilterDto filter);
        Task<List<UserReportDto>> GetAllUserReportAsync();
        Task<List<FinanceReportDto>> GetFinanceReportAsync(FinanceReportFilterDto filter);
        Task<List<FinanceReportDto>> GetAllFinanceReportAsync();

    }
}
