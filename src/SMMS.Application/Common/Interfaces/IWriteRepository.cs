using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SMMS.Application.Common.Interfaces;
/// <summary>
/// 1. Đó refactor thì bổ sung thêm CancellationToken
/// 2. Thêm số method advanced → có thể chưa cần
/// 3. Test lại AddRange, UpdateRange, DeleteRange
/// </summary>
/// <typeparam name="T"></typeparam>
public interface IWriteRepository<T, in TKey> where T : class
{
    Task AddAsync(T entity, CancellationToken ct = default);
    Task AddRangeAsync(IEnumerable<T> entities, CancellationToken ct = default);

    Task UpdateAsync(T entity, CancellationToken ct = default);
    Task UpdateRangeAsync(IEnumerable<T> entities, CancellationToken ct = default);

    Task DeleteAsync(T entity, CancellationToken ct = default);
    Task DeleteByIdAsync(TKey id, CancellationToken ct = default);

    Task SaveChangeAsync(CancellationToken ct = default);
}
