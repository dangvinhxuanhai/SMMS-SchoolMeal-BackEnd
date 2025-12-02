using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediatR;
using SMMS.Application.Features.Menus.DTOs.Schooling;

namespace SMMS.Application.Features.Menus.Queries.Schooling;
public sealed record GetAllSchoolsQuery() : IRequest<IReadOnlyList<SchoolListItemDto>>;
