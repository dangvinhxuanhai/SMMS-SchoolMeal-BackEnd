using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SMMS.Domain.Entities.foodmenu;

namespace SMMS.Application.Features.Meal.Interfaces;
public interface IMenuRepository
{
    Task<Menu> GetWithDetailsAsync(int menuId, CancellationToken ct = default);
    Task AddAsync(Menu menu, CancellationToken ct = default);

    /// <summary>
    /// Lấy danh sách menu theo trường (+ optional năm + tuần).
    /// </summary>
    Task<List<Menu>> GetListBySchoolAsync(
        Guid schoolId,
        int? yearId,
        short? weekNo,
        CancellationToken ct = default);

    /// <summary>
    /// Lấy chi tiết 1 menu theo MenuId + SchoolId, include luôn FoodItems.
    /// </summary>
    Task<Menu?> GetDetailWithFoodAsync(
        int menuId,
        Guid schoolId,
        CancellationToken ct = default);

    /// <summary>
    /// Xóa cứng 1 menu và toàn bộ dữ liệu liên quan.
    /// </summary>
    /// <returns>true nếu xóa được, false nếu không tìm thấy</returns>
    Task<bool> HardDeleteAsync(int menuId, CancellationToken cancellationToken = default);
}
