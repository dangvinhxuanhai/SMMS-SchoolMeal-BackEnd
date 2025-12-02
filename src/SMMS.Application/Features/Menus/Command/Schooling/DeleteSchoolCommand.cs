using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediatR;

namespace SMMS.Application.Features.Menus.Command.Schooling;
public sealed record DeleteSchoolCommand(Guid Id) : IRequest<bool>;
