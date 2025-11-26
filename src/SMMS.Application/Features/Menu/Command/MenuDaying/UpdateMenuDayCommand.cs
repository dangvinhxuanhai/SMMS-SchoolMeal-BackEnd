using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediatR;
using SMMS.Application.Features.Menu.DTOs.MenuDaying;

namespace SMMS.Application.Features.Menu.Command.MenuDaying;
public sealed record UpdateMenuDayCommand(int Id, UpdateMenuDayDto Dto) : IRequest<bool>;
