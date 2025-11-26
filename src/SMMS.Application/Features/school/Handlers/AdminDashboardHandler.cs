using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediatR;
using SMMS.Application.Features.school.DTOs;
using SMMS.Application.Features.school.Interfaces;
using SMMS.Application.Features.school.Queries;

namespace SMMS.Application.Features.school.Handlers
{
    public class GetSystemOverviewQueryHandler : IRequestHandler<GetSystemOverviewQuery, DashboardOverviewDto>
    {
        private readonly IAdminDashboardRepository _adminDashboardRepository;

        public GetSystemOverviewQueryHandler(IAdminDashboardRepository adminDashboardRepository)
        {
            _adminDashboardRepository = adminDashboardRepository;
        }

        public async Task<DashboardOverviewDto> Handle(GetSystemOverviewQuery request, CancellationToken cancellationToken)
        {
            return await _adminDashboardRepository.GetSystemOverviewAsync();
        }
    }
}
