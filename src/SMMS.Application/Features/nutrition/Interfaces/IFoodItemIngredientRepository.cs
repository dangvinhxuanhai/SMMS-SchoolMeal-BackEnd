using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SMMS.Application.Features.Skeleton.Interfaces;
using SMMS.Domain.Entities.nutrition;

namespace SMMS.Application.Features.nutrition.Interfaces;
public interface IFoodItemIngredientRepository
{
    Task<IReadOnlyList<FoodItemIngredient>> GetByFoodIdAsync(
        int foodId,
        CancellationToken cancellationToken = default);


    /// <summary>
    /// Xóa toàn bộ FoodItemIngredients của FoodId và thêm lại danh sách mới.
    /// </summary>
    Task ReplaceForFoodAsync(
        int foodId,
        IEnumerable<FoodItemIngredient> newItems,
        CancellationToken cancellationToken = default);
}
