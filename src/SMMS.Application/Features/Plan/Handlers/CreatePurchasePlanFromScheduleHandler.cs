using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediatR;
using SMMS.Application.Features.Plan.Commands;
using SMMS.Application.Features.Plan.DTOs;
using SMMS.Application.Features.Plan.Interfaces;

namespace SMMS.Application.Features.Plan.Handlers;
public sealed class CreatePurchasePlanFromScheduleHandler
       : IRequestHandler<CreatePurchasePlanFromScheduleCommand, PurchasePlanDto>
{
    private readonly IPurchasePlanRepository _repository;

    public CreatePurchasePlanFromScheduleHandler(IPurchasePlanRepository repository)
    {
        _repository = repository;
    }

    public async Task<PurchasePlanDto> Handle(
        CreatePurchasePlanFromScheduleCommand request,
        CancellationToken cancellationToken)
    {
        // Repo sẽ lo check trùng ScheduleMealId, gen lines từ DailyMeals + MenuFoodItems + FoodItemIngredients
        var planId = await _repository.CreateFromScheduleAsync(
            request.ScheduleMealId,
            request.StaffId,
            cancellationToken);

        // lấy lại detail để trả về
        var dto = await _repository.GetPlanDetailAsync(planId, cancellationToken);
        if (dto == null)
        {
            throw new InvalidOperationException("Failed to load purchase plan after creation.");
        }

        return dto;
    }
}
