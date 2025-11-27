using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediatR;
using SMMS.Application.Features.Menu.DTOs.MenuDaying;

namespace SMMS.Application.Features.Menu.Queries.MenuDaying;
public sealed record GetAllMenuDaysQuery() : IRequest<IReadOnlyList<MenuDayListItemDto>>;
