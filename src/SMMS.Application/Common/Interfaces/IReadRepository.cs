using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace SMMS.Application.Common.Interfaces;
/// <summary>
/// 1. Sau này refactor thì chú ý có thể à upgrade chút
///     - sử dụng TId => bất chấp được double, int, string, guid, ...
///     - thêm 1 số method done read ~ tương tự với write :>
/// 2. Maintain lại IGenericRepository (tạm thời em del full)
/// </summary>
/// <typeparam name="T"></typeparam>
public interface IReadRepository<T, in TKey> where T : class
{
    Task<T?> GetByIdAsync(TKey id, string keyName, CancellationToken ct = default, params Expression<Func<T, object>>[] includes);
    Task<IReadOnlyList<T>> GetAllAsync(CancellationToken ct = default, params Expression<Func<T, object>>[] includes);
    Task<IReadOnlyList<T>> GetByConditionAsync(Expression<Func<T, bool>> predicate, CancellationToken ct = default, params Expression<Func<T, object>>[] includes);
}

