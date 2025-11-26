using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediatR;
using SMMS.Application.Features.Menu.DTOs.Schooling;

namespace SMMS.Application.Features.Menu.Queries.Schooling;
public sealed record GetSchoolByIdQuery(Guid Id) : IRequest<SchoolDetailDto?>;
