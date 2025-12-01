using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediatR;

namespace SMMS.Application.Features.Menus.Command.Menuing;
public sealed record DeleteMenuCommand(int Id) : IRequest<bool>;
