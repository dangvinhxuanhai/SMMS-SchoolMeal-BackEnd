using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediatR;
using SMMS.Application.Features.Plan.DTOs;

namespace SMMS.Application.Features.Plan.Commands;
public sealed record CreatePurchasePlanFromScheduleCommand(
    long ScheduleMealId,
    Guid StaffId
) : IRequest<PurchasePlanDto>;
