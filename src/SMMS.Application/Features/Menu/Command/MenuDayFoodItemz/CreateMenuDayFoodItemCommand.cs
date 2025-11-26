using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediatR;
using SMMS.Application.Features.Menu.DTOs.MenuDayFoodItemz;

namespace SMMS.Application.Features.Menu.Command.MenuDayFoodItemz;
public sealed record CreateMenuDayFoodItemCommand(CreateMenuDayFoodItemDto Dto) : IRequest<(int MenuDayId, int FoodId)>;
