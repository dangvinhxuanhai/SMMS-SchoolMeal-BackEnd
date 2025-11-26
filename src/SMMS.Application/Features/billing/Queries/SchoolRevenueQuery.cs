using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediatR;
using SMMS.Domain.Entities.billing;

namespace SMMS.Application.Features.billing.Queries
{
    public record GetRevenuesBySchoolQuery(Guid SchoolId) : IRequest<IEnumerable<SchoolRevenue>>;
    public record GetRevenueByIdQuery(long RevenueId) : IRequest<SchoolRevenue?>;

}
