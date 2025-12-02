using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SMMS.Domain.Entities.inventory;

namespace SMMS.Application.Features.Inventory.Interfaces;
public interface IInventoryRepository
{
    Task<InventoryItem?> GetByIdAsync(
    int itemId,
    Guid schoolId,
    CancellationToken ct = default);

    Task<int> CountBySchoolAsync(
        Guid schoolId,
        CancellationToken ct = default);

    Task<IReadOnlyList<InventoryItem>> GetPagedBySchoolAsync(
        Guid schoolId,
        int pageIndex,
        int pageSize,
        CancellationToken ct = default);

    /// <summary>
    /// Tìm item kho theo lô (Ingredient + ExpirationDate + BatchNo) trong 1 trường.
    /// Nếu tìm thấy thì cộng dồn QuantityGram, nếu không thì tạo mới.
    /// </summary>
    Task<InventoryItem> AddOrIncreaseAsync(
        Guid schoolId,
        int ingredientId,
        decimal quantityGram,
        DateOnly? expirationDate,
        string? batchNo,
        string? origin,
        Guid? createdBy,
        CancellationToken ct = default);

    /// <summary>
    /// Ghi lại transaction nhập kho (IN).
    /// </summary>
    Task AddInboundTransactionAsync(
        InventoryItem item,
        decimal quantityGram,
        string reference,
        CancellationToken ct = default);

    Task UpdateAsync(InventoryItem item, CancellationToken ct = default);
}
