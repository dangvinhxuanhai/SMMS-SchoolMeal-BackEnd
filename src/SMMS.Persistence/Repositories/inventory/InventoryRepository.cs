using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SMMS.Application.Features.Inventory.Interfaces;
using SMMS.Domain.Entities.inventory;
using SMMS.Persistence.Data;

namespace SMMS.Persistence.Repositories.inventory;
public class InventoryRepository : IInventoryRepository
{
    private readonly EduMealContext _context;

    public InventoryRepository(EduMealContext context)
    {
        _context = context;
    }

    public Task<InventoryItem?> GetByIdAsync(
            int itemId,
            Guid schoolId,
            CancellationToken ct = default)
    {
        return _context.InventoryItems
            .Include(i => i.Ingredient) // nếu entity có navigation
            .FirstOrDefaultAsync(
                x => x.ItemId == itemId && x.SchoolId == schoolId,
                ct);
    }

    public Task<int> CountBySchoolAsync(Guid schoolId, CancellationToken ct = default)
    {
        return _context.InventoryItems
            .Where(x => x.SchoolId == schoolId)
            .CountAsync(ct);
    }

    public async Task<IReadOnlyList<InventoryItem>> GetPagedBySchoolAsync(
        Guid schoolId,
        int pageIndex,
        int pageSize,
        CancellationToken ct = default)
    {
        var skip = (pageIndex - 1) * pageSize;

        return await _context.InventoryItems
            .Include(i => i.Ingredient)
            .Where(x => x.SchoolId == schoolId)
            .OrderBy(x => x.ItemId)
            .Skip(skip)
            .Take(pageSize)
            .ToListAsync(ct);
    }

    public async Task<InventoryItem> AddOrIncreaseAsync(
    Guid schoolId,
    int ingredientId,
    decimal quantityGram,
    DateOnly? expirationDate,
    string? batchNo,
    string? origin,
    Guid? createdBy,
    CancellationToken ct = default)
    {
        // Lấy tên nguyên liệu theo id
        var ingredientInfo = await _context.Ingredients
            .AsNoTracking()
            .Where(i => i.IngredientId == ingredientId)
            .Select(i => new { i.IngredientId, i.IngredientName })
            .FirstOrDefaultAsync(ct);

        if (ingredientInfo == null)
            throw new InvalidOperationException($"Ingredient {ingredientId} not found");

        // Tìm 1 lô hàng trong kho theo Ingredient + ExpirationDate + BatchNo
        var query = _context.InventoryItems
            .Where(x => x.SchoolId == schoolId && x.IngredientId == ingredientId);

        if (expirationDate.HasValue)
        {
            query = query.Where(x => x.ExpirationDate == expirationDate.Value);
        }

        if (!string.IsNullOrWhiteSpace(batchNo))
        {
            query = query.Where(x => x.BatchNo == batchNo);
        }

        var item = await query.FirstOrDefaultAsync(ct);

        var now = DateTime.UtcNow;

        if (item == null)
        {
            item = new InventoryItem
            {
                SchoolId = schoolId,
                IngredientId = ingredientId,
                ItemName = ingredientInfo.IngredientName,
                QuantityGram = quantityGram,
                ExpirationDate = expirationDate,
                BatchNo = batchNo,
                Origin = origin,
                CreatedBy = createdBy,
                CreatedAt = now,
                LastUpdated = now
            };

            await _context.InventoryItems.AddAsync(item, ct);
        }
        else
        {
            // Nếu item cũ chưa có tên thì fill vào luôn
            if (string.IsNullOrEmpty(item.ItemName))
            {
                item.ItemName = ingredientInfo.IngredientName;
            }

            item.QuantityGram += quantityGram;
            item.LastUpdated = now;

            _context.InventoryItems.Update(item);
        }

        return item;
    }


    public async Task AddInboundTransactionAsync(
        InventoryItem item,
        decimal quantityGram,
        string reference,
        CancellationToken ct = default)
    {
        var tx = new InventoryTransaction
        {
            Item = item,
            TransType = "IN",            // nhập kho
            QuantityGram = quantityGram,
            TransDate = DateTime.UtcNow,
            Reference = reference
        };

        await _context.InventoryTransactions.AddAsync(tx, ct);
    }
    public Task UpdateAsync(InventoryItem item, CancellationToken ct = default)
    {
        _context.InventoryItems.Update(item);
        return Task.CompletedTask;
    }
}
