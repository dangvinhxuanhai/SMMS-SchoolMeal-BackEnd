using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SMMS.Application.Features.Manager.Interfaces;
using SMMS.Domain.Entities.billing;
using SMMS.Persistence.Data;
using Microsoft.EntityFrameworkCore;
namespace SMMS.Persistence.Repositories.Manager;
public class ManagerPaymentSettingRepository : IManagerPaymentSettingRepository
{
    private readonly EduMealContext _context;

    public ManagerPaymentSettingRepository(EduMealContext context)
    {
        _context = context;
    }

    public async Task<List<SchoolPaymentSetting>> GetBySchoolAsync(
        Guid schoolId,
        CancellationToken cancellationToken = default)
    {
        return await _context.SchoolPaymentSettings
            .Where(x => x.SchoolId == schoolId)
            .OrderBy(x => x.FromMonth)
            .ThenBy(x => x.ToMonth)
            .ToListAsync(cancellationToken);
    }

    public async Task<SchoolPaymentSetting?> GetByIdAsync(
        int settingId,
        CancellationToken cancellationToken = default)
    {
        return await _context.SchoolPaymentSettings
            .FirstOrDefaultAsync(x => x.SettingId == settingId, cancellationToken);
    }

    public async Task<SchoolPaymentSetting> AddAsync(
        SchoolPaymentSetting entity,
        CancellationToken cancellationToken = default)
    {
        entity.CreatedAt = DateTime.UtcNow;
        entity.IsActive = true;

        _context.SchoolPaymentSettings.Add(entity);
        await _context.SaveChangesAsync(cancellationToken);

        return entity;
    }

    public async Task UpdateAsync(
        SchoolPaymentSetting entity,
        CancellationToken cancellationToken = default)
    {
        _context.SchoolPaymentSettings.Update(entity);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(
        SchoolPaymentSetting entity,
        CancellationToken cancellationToken = default)
    {
        // soft delete
        _context.SchoolPaymentSettings.Remove(entity);
        await _context.SaveChangesAsync(cancellationToken);
    }
    public async Task<bool> HasOverlappedRangeAsync(
        Guid schoolId,
        byte fromMonth,
        byte toMonth,
        int? excludeSettingId = null,
        CancellationToken cancellationToken = default)
    {
        return await _context.SchoolPaymentSettings
            .AnyAsync(x =>
                x.SchoolId == schoolId
                && x.IsActive
                && (excludeSettingId == null || x.SettingId != excludeSettingId.Value)
                // 2 khoảng [fromMonth, toMonth] và [x.FromMonth, x.ToMonth] giao nhau
                && fromMonth <= x.ToMonth
                && toMonth >= x.FromMonth,
                cancellationToken);
    }
    public async Task<SchoolPaymentSetting?> GetExactRangeAsync(
    Guid schoolId,
    short fromMonth,
    short toMonth,
    CancellationToken ct)
    {
        return await _context.SchoolPaymentSettings
            .AsNoTracking()
            .FirstOrDefaultAsync(
                x => x.SchoolId == schoolId
                     && x.IsActive
                     && x.FromMonth == fromMonth
                     && x.ToMonth == toMonth,
                ct);
    }
}
