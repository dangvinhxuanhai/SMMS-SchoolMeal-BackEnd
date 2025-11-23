using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SMMS.Application.Features.Skeleton.Interfaces;
using SMMS.Domain.Entities.nutrition;

namespace SMMS.Application.Features.nutrition.Interfaces;
public interface IFoodItemRepository
{
    Task<FoodItem?> GetByIdAsync(int id, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<FoodItem>> GetListAsync(
        Guid schoolId,
        string? keyword,
        bool includeInactive = false,
        CancellationToken cancellationToken = default);

    Task AddAsync(FoodItem entity, CancellationToken cancellationToken = default);

    Task UpdateAsync(FoodItem entity, CancellationToken cancellationToken = default);

    /// <summary>
    /// Soft delete (set IsActive = false)
    /// </summary>
    Task SoftDeleteAsync(FoodItem entity, CancellationToken cancellationToken = default);

    /// <summary>
    /// Dùng nếu bạn muốn hard delete nhưng phải check quan hệ
    /// </summary>
    Task<bool> HasRelationsAsync(int foodId, CancellationToken cancellationToken = default);

    Task HardDeleteAsync(FoodItem entity, CancellationToken cancellationToken = default);
}
