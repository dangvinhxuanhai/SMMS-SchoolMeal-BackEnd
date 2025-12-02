using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Http;
using SMMS.Application.Features.school.DTOs;
using SMMS.Domain.Entities.school;

namespace SMMS.Application.Features.school.Commands
{
    public record CreateSchoolCommand(CreateSchoolDto SchoolDto, Guid CreatedBy) : IRequest<Guid>;
    public record UpdateSchoolCommand(Guid SchoolId, UpdateSchoolDto SchoolDto, Guid UpdatedBy) : IRequest<Unit>;
    public record DeleteSchoolCommand(Guid SchoolId) : IRequest<Unit>;
}
