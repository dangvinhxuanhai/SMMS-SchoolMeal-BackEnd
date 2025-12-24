using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SMMS.Application.Features.Meal.Interfaces;
public interface IDailyMealEvidenceRepository
{
    Task<long> CreateAsync(
        int dailyMealId,
        string evidenceUrl,
        string? caption,
        Guid uploadedBy,
        CancellationToken ct);

    Task DeleteAsync(long evidenceId, CancellationToken ct);
}
