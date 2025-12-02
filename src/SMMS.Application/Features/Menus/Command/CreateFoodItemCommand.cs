using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediatR;
using SMMS.Application.Features.Menus.DTOs;

namespace SMMS.Application.Features.Menus.Command;
public record CreateFoodItemCommand(CreateFoodItemDto Dto) : IRequest<int>;

