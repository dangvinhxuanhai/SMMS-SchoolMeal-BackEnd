using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediatR;
using SMMS.Application.Features.Menu.DTOs.Schooling;

namespace SMMS.Application.Features.Menu.Command.Schooling;
public sealed record CreateSchoolCommand(CreateSchoolDto Dto) : IRequest<Guid>;
