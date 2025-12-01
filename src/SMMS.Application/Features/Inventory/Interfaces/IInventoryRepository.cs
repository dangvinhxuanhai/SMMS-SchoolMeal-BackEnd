using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SMMS.Domain.Entities.inventory;

namespace SMMS.Application.Features.Inventory.Interfaces;
public interface IInventoryRepository
{
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
        int itemId,
        decimal quantityGram,
        string reference,
        CancellationToken ct = default);
}
