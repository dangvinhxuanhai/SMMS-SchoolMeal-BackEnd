using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using DocumentFormat.OpenXml.Vml.Office;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Diagnostics;
using SMMS.Application.Abstractions;
using SMMS.Domain.Entities.Logs;

namespace SMMS.Persistence.Interceptors;
public class AuditSaveChangesInterceptor : SaveChangesInterceptor
{
    private readonly ICurrentUserService _currentUser;

    private static readonly HashSet<string> IgnoredTables = new(StringComparer.OrdinalIgnoreCase)
    {
        "AuditLogs",
        "RefreshTokens",
        "LoginAttempts",
        "UserExternalLogins"
    };

    private static readonly HashSet<string> IgnoredProperties = new(StringComparer.OrdinalIgnoreCase)
    {
        "PasswordHash",
        "Token",
        "RefreshToken",
        "ConcurrencyStamp"
    };

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

        var userId = _currentUser.UserId ?? Guid.Empty; // Guid? (nullable OK)

        var auditLogs = new List<AuditLog>();

        foreach (var entry in context.ChangeTracker.Entries())
        {
            if (!IsAuditable(entry)) continue;

            var tableName = entry.Metadata.GetTableName();
            if (tableName == null || IgnoredTables.Contains(tableName)) continue;

            var recordId = GetPrimaryKey(entry);
            var changes = GetFieldChanges(entry);

            if (!changes.Any()) continue;

            var audit = new AuditLog
            {
                LogId = Guid.NewGuid(),
                UserId = userId, // nullable
                ActionType = entry.State.ToString(), // Added / Modified / Deleted
                ActionDesc = BuildActionDesc(entry.State, tableName, recordId, changes),
                TableName = tableName,
                RecordId = recordId,
                OldData = entry.State == EntityState.Added ? null : SerializeOriginal(entry),
                NewData = entry.State == EntityState.Deleted ? null : SerializeCurrent(entry),
                CreatedAt = DateTime.UtcNow
            };

            auditLogs.Add(audit);
        }

        if (auditLogs.Any())
        {
            context.Set<AuditLog>().AddRange(auditLogs);
        }

        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    // =========================
    // Helpers
    // =========================

    private static bool IsAuditable(EntityEntry entry)
    {
        return entry.State == EntityState.Added
            || entry.State == EntityState.Modified
            || entry.State == EntityState.Deleted;
    }

    private static string? GetPrimaryKey(EntityEntry entry)
    {
        var key = entry.Properties.FirstOrDefault(p => p.Metadata.IsPrimaryKey());
        return key?.CurrentValue?.ToString();
    }

    private static List<string> GetFieldChanges(EntityEntry entry)
    {
        var changes = new List<string>();

        foreach (var prop in entry.Properties)
        {
            if (prop.Metadata.IsPrimaryKey()) continue;
            if (prop.Metadata.IsShadowProperty()) continue;
            if (IgnoredProperties.Contains(prop.Metadata.Name)) continue;

            var name = prop.Metadata.Name;
            var original = prop.OriginalValue?.ToString();
            var current = prop.CurrentValue?.ToString();

            switch (entry.State)
            {
                case EntityState.Added:
                    if (current != null)
                        changes.Add($"+ {name} = {current}");
                    break;

                case EntityState.Deleted:
                    if (original != null)
                        changes.Add($"- {name} = {original}");
                    break;

                case EntityState.Modified:
                    if (original != current)
                        changes.Add($"~ {name}: {original} → {current}");
                    break;
            }
        }

        return changes;
    }

    private static string BuildActionDesc(
        EntityState state,
        string tableName,
        string? recordId,
        List<string> changes)
    {
        var prefix = state switch
        {
            EntityState.Added => "Create",
            EntityState.Modified => "Update",
            EntityState.Deleted => "Delete",
            _ => "Change"
        };

        var idPart = recordId != null ? $" (Id={recordId})" : string.Empty;
        var detail = string.Join("; ", changes);

        return $"{prefix} {tableName}{idPart}: {detail}";
    }

    private static string SerializeOriginal(EntityEntry entry)
    {
        var data = entry.OriginalValues.Properties
            .Where(p => !IgnoredProperties.Contains(p.Name))
            .ToDictionary(
                p => p.Name,
                p => entry.OriginalValues[p]?.ToString());

        return JsonSerializer.Serialize(data);
    }

    private static string SerializeCurrent(EntityEntry entry)
    {
        var data = entry.CurrentValues.Properties
            .Where(p => !IgnoredProperties.Contains(p.Name))
            .ToDictionary(
                p => p.Name,
                p => entry.CurrentValues[p]?.ToString());

        return JsonSerializer.Serialize(data);
    }
}
