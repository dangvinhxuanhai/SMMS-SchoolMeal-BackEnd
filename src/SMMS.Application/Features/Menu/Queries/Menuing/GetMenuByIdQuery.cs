using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediatR;
using SMMS.Application.Features.Menu.DTOs.Menuing;

namespace SMMS.Application.Features.Menu.Queries.Menuing;
public sealed record GetMenuByIdQuery(int Id) : IRequest<MenuDetailDto?>;
