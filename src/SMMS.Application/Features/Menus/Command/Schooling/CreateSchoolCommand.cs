using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediatR;
using SMMS.Application.Features.Menus.DTOs.Schooling;

namespace SMMS.Application.Features.Menus.Command.Schooling;
public sealed record CreateSchoolCommand(CreateSchoolDto Dto) : IRequest<Guid>;
