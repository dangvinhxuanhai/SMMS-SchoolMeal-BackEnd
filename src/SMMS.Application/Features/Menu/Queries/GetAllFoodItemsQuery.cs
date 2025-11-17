using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediatR;
using SMMS.Application.Features.Menu.DTOs;
using SMMS.Domain.Entities.nutrition;

namespace SMMS.Application.Features.Menu.Queries;
public record GetAllFoodItemsQuery : IRequest<IReadOnlyList<FoodItemDto>>;
