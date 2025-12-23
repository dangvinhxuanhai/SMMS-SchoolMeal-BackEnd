using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SMMS.Application.Features.Inventory.DTOs;

namespace SMMS.Application.Features.Inventory.Interfaces;
public interface IInventoryConsumptionRepository
{
    Task<List<InventoryConsumeItemResult>> ConsumeByScheduleAsync(
        long scheduleMealId,
        Guid executedBy,
        CancellationToken ct);
}
