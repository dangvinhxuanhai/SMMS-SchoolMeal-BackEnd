using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SMMS.Application.Common.Interfaces;
using SMMS.Persistence.Data;

namespace SMMS.Persistence.Repositories.Skeleton;
public class EFReadRepository<T, TKey> : IReadRepository<T, TKey> where T : class
{
    protected readonly EduMealContext _context;
    protected readonly DbSet<T> _dbSet;

    public EFReadRepository(EduMealContext context)
    {
        _context = context;
        _dbSet = _context.Set<T>();
    }

    public virtual async Task<IReadOnlyList<T>> GetAllAsync(CancellationToken ct = default, params Expression<Func<T, object>>[] includes)
    {
        IQueryable<T> query = _dbSet.AsQueryable();
        foreach (var include in includes)
        {
            query = query.Include(include);
        }
        return await query.ToListAsync(ct);
    }

    public virtual async Task<IReadOnlyList<T>> GetByConditionAsync(Expression<Func<T, bool>> predicate, CancellationToken ct = default, params Expression<Func<T, object>>[] includes)
    {
        IQueryable<T> query = _dbSet.Where(predicate);
        foreach (var include in includes)
        {
            query = query.Include(include);
        }
        return await query.ToListAsync(ct);
    }
    /// <summary>
    /// ko sử dụng "Id" vì mỗi entities ko hề chung pk name keyword
    /// → sử dụng string thế vào để dò, hơi cồng kềnh xíu đều đỡ code nhiều :v
    /// test ver FindAsync()
    /// </summary>
    /// <param name="id"></param>
    /// <param name="ct"></param>
    /// <param name="includes"></param>
    /// <returns></returns>
    public virtual async Task<T?> GetByIdAsync(TKey id, string keyName, CancellationToken ct = default, params Expression<Func<T, object>>[] includes)
    {
        IQueryable<T> query = _dbSet.AsQueryable();
        foreach (var include in includes)
        {
            query = query.Include(include);
        }
        //var entity = await query.FirstOrDefaultAsync(e => EF.Property<TKey>(e, "Id")!.Equals(id), ct);
        var entity = await query.FirstOrDefaultAsync(
                        e => EF.Property<TKey>(e, keyName)!.Equals(id),
                        ct);
        //var entity = await query.FirstAsync(e => EF.Property<TKey>(e, keyName)!.Equals(id), ct);
        return entity;
    }
}
