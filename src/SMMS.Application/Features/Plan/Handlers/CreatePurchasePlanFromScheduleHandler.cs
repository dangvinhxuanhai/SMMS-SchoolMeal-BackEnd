using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediatR;
using SMMS.Application.Features.Inventory.Interfaces;
using SMMS.Application.Features.Plan.Commands;
using SMMS.Application.Features.Plan.DTOs;
using SMMS.Application.Features.Plan.Interfaces;

namespace SMMS.Application.Features.Plan.Handlers;
public sealed class CreatePurchasePlanFromScheduleHandler
       : IRequestHandler<CreatePurchasePlanFromScheduleCommand, CreatePurchasePlanResultDto>
{
    private readonly IPurchasePlanRepository _repository;

    public CreatePurchasePlanFromScheduleHandler(IPurchasePlanRepository repository)
    {
        _repository = repository;

    }

    public async Task<CreatePurchasePlanResultDto> Handle(
        CreatePurchasePlanFromScheduleCommand request,
        CancellationToken cancellationToken)
    {
        var result = await _repository.CreateFromScheduleAsync(
            request.ScheduleMealId,
            request.StaffId,
            cancellationToken);

        // ❌ Không tạo plan → trả result cho FE xử lý điều hướng
        if (!result.IsCreated)
        {
            return new CreatePurchasePlanResultDto
            {
                IsCreated = false,
                PurchasePlanId = null,
                Reason = result.Reason,
                Message = result.Message,
                Plan = null
            };
        }

        // ✅ Có tạo plan → load detail trả về
        var planDto = await _repository
            .GetPlanDetailAsync(result.PurchasePlanId!.Value, cancellationToken);

        if (planDto == null)
        {
            throw new InvalidOperationException(
                "Purchase plan created but failed to load detail.");
        }

        return new CreatePurchasePlanResultDto
        {
            IsCreated = true,
            PurchasePlanId = result.PurchasePlanId,
            Reason = "Created",
            Message = "Purchase plan được tạo thành công",
            Plan = planDto
        };
    }
}
