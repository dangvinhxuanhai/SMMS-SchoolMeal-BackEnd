using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediatR;
using SMMS.Application.Features.auth.DTOs;
using SMMS.Application.Features.auth.Interfaces;
using SMMS.Application.Features.auth.Queries;

namespace SMMS.Application.Features.auth.Handlers
{
    public class ReportQueryHandler :
        IRequestHandler<GetUserReportQuery, List<UserReportDto>>,
        IRequestHandler<GetAllUserReportQuery, List<UserReportDto>>
    {
        private readonly IReportRepository _reportRepository;

        public ReportQueryHandler(IReportRepository reportRepository)
        {
            _reportRepository = reportRepository;
        }

        public async Task<List<UserReportDto>> Handle(GetUserReportQuery request, CancellationToken cancellationToken)
        {
            return await _reportRepository.GetUserReportAsync(request.Filter);
        }

        public async Task<List<UserReportDto>> Handle(GetAllUserReportQuery request, CancellationToken cancellationToken)
        {
            return await _reportRepository.GetAllUserReportAsync();
        }
    }
}
