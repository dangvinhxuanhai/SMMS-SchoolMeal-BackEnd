using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediatR;
using SMMS.Application.Features.Menus.DTOs;
using SMMS.Domain.Entities.nutrition;

namespace SMMS.Application.Features.Menus.Queries;
public record GetAllFoodItemsQuery : IRequest<IReadOnlyList<FoodItemDto>>;
