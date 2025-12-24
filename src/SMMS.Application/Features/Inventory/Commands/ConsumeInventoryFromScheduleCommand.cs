using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediatR;
using SMMS.Application.Features.Inventory.DTOs;

namespace SMMS.Application.Features.Inventory.Commands;
public record ConsumeInventoryFromScheduleCommand(
    long ScheduleMealId,
    Guid ExecutedBy
) : IRequest<ConsumeInventoryResult>;
