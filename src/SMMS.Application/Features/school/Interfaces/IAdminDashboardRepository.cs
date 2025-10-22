using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SMMS.Application.Features.school.DTOs;

namespace SMMS.Application.Features.school.Interfaces
{
    public interface IAdminDashboardRepository
    {
        Task<DashboardOverviewDto> GetSystemOverviewAsync();
    }
}
