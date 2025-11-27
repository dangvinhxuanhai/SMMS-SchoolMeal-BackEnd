using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SMMS.Application.Common.Interfaces;
using SMMS.Persistence.Data;

namespace SMMS.Persistence.Repositories.Skeleton;
public class EFWriteRepository<T, TKey> : IWriteRepository<T, TKey> where T : class
{
    protected readonly EduMealContext _context;
    protected readonly DbSet<T> _dbSet;

    public EFWriteRepository(EduMealContext context)
    {
        _context = context;
        _dbSet = _context.Set<T>();
    }
    #region CUD
    public virtual async Task AddAsync(T entity, CancellationToken ct = default)
    {
        await _dbSet.AddAsync(entity, ct);
    }

    public virtual async Task AddRangeAsync(IEnumerable<T> entities, CancellationToken ct = default)
    {
        await _dbSet.AddRangeAsync(entities, ct);
    }

    public virtual Task UpdateAsync(T entity, CancellationToken ct = default)
    {
        _dbSet.Update(entity);
        return Task.CompletedTask;
    }

    public virtual Task UpdateRangeAsync(IEnumerable<T> entities, CancellationToken ct = default)
    {
        _dbSet.UpdateRange(entities);
        return Task.CompletedTask;
    }

    public virtual Task DeleteAsync(T entity, CancellationToken ct = default)
    {
        if (entity == null)
            throw new ArgumentNullException(nameof(entity),
                        $"Entity {typeof(T).Name} must not be null");

        _dbSet.Remove(entity);
        return Task.CompletedTask;
    }

    public virtual async Task DeleteByIdAsync(TKey id, CancellationToken ct = default)
    {
        var entity = await _dbSet.FindAsync(new object[] { id }, ct);
        if (entity == null)
            throw new KeyNotFoundException(
                $"{typeof(T).Name} with id {id} was not found");

        _dbSet.Remove(entity);
    }
    #endregion

    public virtual async Task SaveChangeAsync(CancellationToken ct = default)
    {
        await _context.SaveChangesAsync(ct);
    }
}
