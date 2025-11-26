using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediatR;
using SMMS.Application.Features.auth.DTOs;

namespace SMMS.Application.Features.auth.Queries
{
    // ✅ Lấy báo cáo theo bộ lọc (theo trường, thời gian, v.v.)
    public record GetUserReportQuery(ReportFilterDto Filter) : IRequest<List<UserReportDto>>;

    // ✅ Lấy toàn bộ báo cáo người dùng (không lọc)
    public record GetAllUserReportQuery() : IRequest<List<UserReportDto>>;
    public record GetFinanceReportQuery(FinanceReportFilterDto Filter) : IRequest<List<FinanceReportDto>>;
    public record GetAllFinanceReportQuery() : IRequest<List<FinanceReportDto>>;
}
