using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediatR;

namespace SMMS.Application.Features.Menus.Command.MenuDaying;
public sealed record DeleteMenuDayCommand(int Id) : IRequest<bool>;
