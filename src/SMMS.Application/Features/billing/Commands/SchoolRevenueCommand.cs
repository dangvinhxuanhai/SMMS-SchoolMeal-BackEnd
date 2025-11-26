using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediatR;
using SMMS.Application.Features.billing.DTOs;

namespace SMMS.Application.Features.billing.Commands
{
    public record CreateSchoolRevenueCommand(CreateSchoolRevenueDto Dto) : IRequest<long>;
    public record UpdateSchoolRevenueCommand(long RevenueId, UpdateSchoolRevenueDto Dto) : IRequest<Unit>;

    public record DeleteSchoolRevenueCommand(long RevenueId) : IRequest<Unit>;


}
