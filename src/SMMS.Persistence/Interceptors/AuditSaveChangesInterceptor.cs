using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using SMMS.Application.Abstractions;
using SMMS.Domain.Entities.Logs;

namespace SMMS.Persistence.Interceptors;
public class AuditSaveChangesInterceptor : SaveChangesInterceptor
{
    private readonly ICurrentUserService _currentUser;

    public AuditSaveChangesInterceptor(ICurrentUserService currentUser)
    {
        _currentUser = currentUser;
    }

    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        var context = eventData.Context;
        if (context == null) return base.SavingChangesAsync(eventData, result, cancellationToken);

        var userId = _currentUser.UserId ?? Guid.Empty;

        var auditLogs = context.ChangeTracker.Entries()
            .Where(e =>
                e.State == EntityState.Added ||
                e.State == EntityState.Modified ||
                e.State == EntityState.Deleted)
            .Select(e =>
            {
                var tableName = e.Metadata.GetTableName();
                var key = e.Properties.FirstOrDefault(p => p.Metadata.IsPrimaryKey());
                var actionDesc = e.State switch
                {
                    EntityState.Added => $"Create {tableName}",
                    EntityState.Modified => $"Update {tableName}",
                    EntityState.Deleted => $"Delete {tableName}",
                    _ => $"Change {tableName}"
                };

                return new AuditLog
                {
                    LogId = Guid.NewGuid(),
                    UserId = userId,
                    ActionType = e.State.ToString(), // Added / Modified / Deleted
                    ActionDesc = actionDesc ?? "khong xac dinh",         // ✅ KHÔNG NULL
                    RecordId = key?.CurrentValue?.ToString(),
                    OldData = e.State == EntityState.Added
                        ? null
                        : JsonSerializer.Serialize(
                            e.OriginalValues.Properties.ToDictionary(
                                p => p.Name,
                                p => e.OriginalValues[p])),
                    NewData = e.State == EntityState.Deleted
                        ? null
                        : JsonSerializer.Serialize(
                            e.CurrentValues.Properties.ToDictionary(
                                p => p.Name,
                                p => e.CurrentValues[p])),
                    CreatedAt = DateTime.UtcNow
                };
            })
            .ToList();

        if (auditLogs.Any())
        {
            context.Set<AuditLog>().AddRange(auditLogs);
        }

        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }
}
