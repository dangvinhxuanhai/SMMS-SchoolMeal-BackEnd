using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediatR;
using SMMS.Application.Features.Menus.DTOs.MenuDayFoodItemz;

namespace SMMS.Application.Features.Menus.Queries.MenuDayFoodItemz;
public sealed record GetMenuDayFoodItemByIdQuery(int MenuDayId, int FoodId)
    : IRequest<MenuDayFoodItemDetailDto?>;
