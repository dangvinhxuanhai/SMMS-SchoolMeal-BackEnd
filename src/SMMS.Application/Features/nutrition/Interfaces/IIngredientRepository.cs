using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SMMS.Application.Features.Skeleton.Interfaces;
using SMMS.Domain.Entities.nutrition;

namespace SMMS.Application.Features.nutrition.Interfaces;
public interface IIngredientRepository
{
    Task<Ingredient?> GetByIdAsync(int id, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Ingredient>> GetListAsync(
        Guid schoolId,
        string? keyword,
        bool includeInactive = false,
        CancellationToken cancellationToken = default);

    Task AddAsync(Ingredient entity, CancellationToken cancellationToken = default);
    Task UpdateAsync(Ingredient entity, CancellationToken cancellationToken = default);

    Task SoftDeleteAsync(Ingredient entity, CancellationToken cancellationToken = default);

    /// <summary>
    /// Xóa tất cả quan hệ ở bảng liên quan rồi xóa Ingredient (hard delete).
    /// </summary>
    Task HardDeleteWithRelationsAsync(Ingredient entity, CancellationToken cancellationToken = default);
}
