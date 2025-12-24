using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediatR;
using SMMS.Application.Features.Meal.DTOs;

namespace SMMS.Application.Features.Meal.Command;
public record UpdateDailyMealActualIngredientsCommand(
    int DailyMealId,
    List<DailyMealActualIngredientUpdateDto> Items
) : IRequest;
