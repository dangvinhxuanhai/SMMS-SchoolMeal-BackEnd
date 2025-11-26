using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediatR;
using SMMS.Application.Features.school.DTOs;
using SMMS.Domain.Entities.school;

namespace SMMS.Application.Features.school.Queries
{
    public record GetSchoolByIdQuery(Guid SchoolId) : IRequest<SchoolDTO?>;
    public record GetAllSchoolsQuery() : IRequest<IEnumerable<SchoolDTO>>;
}
