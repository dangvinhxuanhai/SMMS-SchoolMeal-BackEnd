using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediatR;
using SMMS.Application.Features.Menus.DTOs.MenuDaying;

namespace SMMS.Application.Features.Menus.Command.MenuDaying;
public sealed record CreateMenuDayCommand(CreateMenuDayDto Dto) : IRequest<int>;
