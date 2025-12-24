using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SMMS.Application.Features.Meal.Interfaces;
using SMMS.Domain.Entities.foodmenu;
using SMMS.Persistence.Data;

namespace SMMS.Persistence.Repositories.Meal;

public class DailyMealEvidenceRepository : IDailyMealEvidenceRepository
{
    private readonly EduMealContext _context;

    public DailyMealEvidenceRepository(EduMealContext context)
    {
        _context = context;
    }

    public async Task<long> CreateAsync(
        int dailyMealId,
        string evidenceUrl,
        string? caption,
        Guid uploadedBy,
        CancellationToken ct)
    {
        var entity = new DailyMealEvidence
        {
            DailyMealId = dailyMealId,
            EvidenceUrl = evidenceUrl,
            Caption = caption,
            UploadedBy = uploadedBy,
            UploadedAt = DateTime.UtcNow
        };

        _context.DailyMealEvidences.Add(entity);
        await _context.SaveChangesAsync(ct);

        return entity.EvidenceId;
    }

    public async Task DeleteAsync(long evidenceId, CancellationToken ct)
    {
        var entity = await _context.DailyMealEvidences
            .FirstOrDefaultAsync(x => x.EvidenceId == evidenceId, ct);

        if (entity == null)
            throw new InvalidOperationException("DailyMealEvidence not found.");

        _context.DailyMealEvidences.Remove(entity);
        await _context.SaveChangesAsync(ct);
    }
}
