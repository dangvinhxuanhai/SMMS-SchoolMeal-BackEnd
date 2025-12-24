using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediatR;
using SMMS.Application.Features.Meal.Command;
using SMMS.Application.Features.Meal.Interfaces;

namespace SMMS.Application.Features.Meal.Handlers;
public class UpdateDailyMealActualIngredientsCommandHandler
    : IRequestHandler<UpdateDailyMealActualIngredientsCommand>
{
    private readonly IDailyMealActualIngredientRepository _repo;

    public UpdateDailyMealActualIngredientsCommandHandler(
        IDailyMealActualIngredientRepository repo)
    {
        _repo = repo;
    }

    public async Task Handle(
        UpdateDailyMealActualIngredientsCommand request,
        CancellationToken ct)
    {
        await _repo.BulkUpdateAsync(
            request.DailyMealId,
            request.Items,
            ct);
    }
}

