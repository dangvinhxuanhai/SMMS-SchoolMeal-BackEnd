using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediatR;
using SMMS.Application.Features.school.DTOs;

namespace SMMS.Application.Features.school.Queries
{
    public class GetSystemOverviewQuery : IRequest<DashboardOverviewDto>
    {
        // Nếu sau này có tham số lọc (ví dụ theo tháng, năm) thì thêm vào đây
    }
}
