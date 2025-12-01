using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediatR;
using SMMS.Application.Features.Menus.DTOs.Menuing;

namespace SMMS.Application.Features.Menus.Command.Menuing;
public sealed record UpdateMenuCommand(int Id, UpdateMenuDto Dto) : IRequest<bool>;
