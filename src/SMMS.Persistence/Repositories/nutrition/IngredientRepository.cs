using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SMMS.Application.Features.nutrition.Interfaces;
using SMMS.Domain.Entities.nutrition;
using SMMS.Persistence.Data;
using SMMS.Persistence.Repositories.Skeleton;

namespace SMMS.Persistence.Repositories.nutrition;
public class IngredientRepository : IIngredientRepository
{
    private readonly EduMealContext _context;

    public IngredientRepository(EduMealContext context)
    {
        _context = context;
    }

    public Task<Ingredient?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return _context.Ingredients
            .FirstOrDefaultAsync(x => x.IngredientId == id, cancellationToken);
    }

    public async Task<IReadOnlyList<Ingredient>> GetListAsync(
        Guid schoolId,
        string? keyword,
        bool includeInactive = false,
        CancellationToken cancellationToken = default)
    {
        var query = _context.Ingredients
            .Where(x => x.SchoolId == schoolId);

        if (!includeInactive)
        {
            query = query.Where(x => x.IsActive);
        }

        if (!string.IsNullOrWhiteSpace(keyword))
        {
            query = query.Where(x => x.IngredientName.Contains(keyword));
        }

        return await query
            .OrderBy(x => x.IngredientName)
            .ToListAsync(cancellationToken);
    }

    public async Task AddAsync(Ingredient entity, CancellationToken cancellationToken = default)
    {
        await _context.Ingredients.AddAsync(entity, cancellationToken);
    }

    public Task UpdateAsync(Ingredient entity, CancellationToken cancellationToken = default)
    {
        _context.Ingredients.Update(entity);
        return Task.CompletedTask;
    }

    public Task SoftDeleteAsync(Ingredient entity, CancellationToken cancellationToken = default)
    {
        entity.IsActive = false;
        _context.Ingredients.Update(entity);
        return Task.CompletedTask;
    }

    public async Task HardDeleteWithRelationsAsync(
    Ingredient entity,
    CancellationToken cancellationToken = default)
    {
        var ingredientId = entity.IngredientId;

        // 1) IngredientAlternatives (cả IngredientId lẫn AltIngredientId)
        var ingredientAlternatives = _context.IngredientAlternatives
            .Where(x => x.IngredientId == ingredientId || x.AltIngredientId == ingredientId);
        _context.IngredientAlternatives.RemoveRange(ingredientAlternatives);

        // 2) AllergeticIngredients
        var allergeticIngredients = _context.AllergeticIngredients
            .Where(x => x.IngredientId == ingredientId);
        _context.AllergeticIngredients.RemoveRange(allergeticIngredients);

        // 3) FoodItemIngredients
        var foodItemIngredients = _context.FoodItemIngredients
            .Where(x => x.IngredientId == ingredientId);
        _context.FoodItemIngredients.RemoveRange(foodItemIngredients);

        // 4) IngredientInFridge – dùng raw SQL vì chưa có DbSet
        await _context.Database.ExecuteSqlRawAsync(
            "DELETE FROM fridge.IngredientInFridge WHERE IngredientId = {0}",
            ingredientId);

        // 5) PurchasePlanLines
        var purchasePlanLines = _context.PurchasePlanLines
            .Where(x => x.IngredientId == ingredientId);
        _context.PurchasePlanLines.RemoveRange(purchasePlanLines);

        // 6) PurchaseOrderLines
        var purchaseOrderLines = _context.PurchaseOrderLines
            .Where(x => x.IngredientId == ingredientId);
        _context.PurchaseOrderLines.RemoveRange(purchaseOrderLines);

        // 7) InventoryItems + InventoryTransactions
        var inventoryItems = await _context.InventoryItems
            .Where(x => x.IngredientId == ingredientId)
            .ToListAsync(cancellationToken);

        if (inventoryItems.Count > 0)
        {
            var itemIds = inventoryItems.Select(i => i.ItemId).ToList();

            var inventoryTrans = _context.InventoryTransactions
                .Where(t => itemIds.Contains(t.ItemId));
            _context.InventoryTransactions.RemoveRange(inventoryTrans);

            _context.InventoryItems.RemoveRange(inventoryItems);
        }

        // 8) Cuối cùng: xóa chính Ingredient
        _context.Ingredients.Remove(entity);
    }
}
