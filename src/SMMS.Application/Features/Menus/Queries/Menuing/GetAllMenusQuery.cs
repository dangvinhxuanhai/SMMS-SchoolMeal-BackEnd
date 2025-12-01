using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediatR;
using SMMS.Application.Features.Menus.DTOs.Menuing;

namespace SMMS.Application.Features.Menus.Queries.Menuing;
public sealed record GetAllMenusQuery() : IRequest<IReadOnlyList<MenuListItemDto>>;
