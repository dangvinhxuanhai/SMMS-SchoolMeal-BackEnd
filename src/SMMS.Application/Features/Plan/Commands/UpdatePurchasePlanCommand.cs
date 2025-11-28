using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediatR;
using SMMS.Application.Features.Plan.DTOs;

namespace SMMS.Application.Features.Plan.Commands;
public sealed record UpdatePurchasePlanCommand(
    int PlanId,
    string PlanStatus,                          // Draft / Confirmed / Exported
    Guid? ConfirmedBy,                          // null nếu chưa confirm
    List<UpdatePurchasePlanLineDto> Lines      // danh sách line mới, replace toàn bộ
) : IRequest<PurchasePlanDto>;
