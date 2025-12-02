using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediatR;
using SMMS.Application.Features.school.DTOs;

namespace SMMS.Application.Features.school.Queries
{
    public record GetImagesByStudentQuery(Guid StudentId) : IRequest<List<StudentImageDto>>;
}
