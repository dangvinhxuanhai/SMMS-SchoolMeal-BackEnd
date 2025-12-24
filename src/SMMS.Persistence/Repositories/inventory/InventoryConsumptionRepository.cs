using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SMMS.Application.Features.Inventory.DTOs;
using SMMS.Application.Features.Inventory.Interfaces;
using SMMS.Domain.Entities.inventory;
using SMMS.Persistence.Data;

namespace SMMS.Persistence.Repositories.inventory;
public class InventoryConsumptionRepository
    : IInventoryConsumptionRepository
{
    private readonly EduMealContext _context;

    public InventoryConsumptionRepository(EduMealContext context)
    {
        _context = context;
    }

    public async Task<List<InventoryConsumeItemResult>> ConsumeByScheduleAsync(
        long scheduleMealId,
        Guid executedBy,
        CancellationToken ct)
    {
        var schedule = await _context.ScheduleMeals
            .FirstOrDefaultAsync(s => s.ScheduleMealId == scheduleMealId, ct);

        if (schedule == null)
            throw new InvalidOperationException("ScheduleMeal not found.");

        // ❌ ĐÃ TRỪ KHO RỒI → KHÔNG CHO TRỪ LẠI
        if (schedule.IsInventoryDeducted)
            throw new InvalidOperationException(
                "Inventory for this schedule has already been deducted.");

        // 1️⃣ Tổng hợp actual theo ingredient
        using var tx = await _context.Database.BeginTransactionAsync(ct);
        try
        {
            var actualByIngredient = await (
                from dm in _context.DailyMeals
                join ai in _context.DailyMealActualIngredients
                    on dm.DailyMealId equals ai.DailyMealId
                join ing in _context.Ingredients
                    on ai.IngredientId equals ing.IngredientId
                where dm.ScheduleMealId == scheduleMealId
                group new { ai, ing } by new
                {
                    ai.IngredientId,
                    ing.IngredientName
                }
                into g
                select new
                {
                    IngredientId = g.Key.IngredientId,
                    IngredientName = g.Key.IngredientName,
                    RequiredGram = g.Sum(x => x.ai.ActualQtyGram)
                }
            ).ToListAsync(ct);

            var results = new List<InventoryConsumeItemResult>();

            // 2️⃣ Trừ kho theo từng ingredient
            foreach (var req in actualByIngredient)
            {
                var inventoryItems = await _context.InventoryItems
                    .Where(i =>
                        i.IngredientId == req.IngredientId &&
                        i.IsActive &&
                        (i.ExpirationDate == null || i.ExpirationDate >= DateOnly.FromDateTime(DateTime.UtcNow)))
                    .OrderBy(i => i.ExpirationDate) // FIFO / FEFO
                    .ToListAsync(ct);

                var available = inventoryItems.Sum(x => x.QuantityGram);
                var remaining = req.RequiredGram;

                foreach (var item in inventoryItems)
                {
                    if (remaining <= 0)
                        break;

                    var consume = Math.Min(item.QuantityGram, remaining);

                    item.QuantityGram -= consume;
                    remaining -= consume;

                    _context.InventoryTransactions.Add(new InventoryTransaction
                    {
                        ItemId = item.ItemId,
                        TransType = "OUT",
                        QuantityGram = consume,
                        Reference = $"ScheduleMeal:{scheduleMealId}",
                        TransDate = DateTime.UtcNow
                    });
                }

                results.Add(new InventoryConsumeItemResult
                {
                    IngredientId = req.IngredientId,
                    IngredientName = req.IngredientName,
                    RequiredGram = req.RequiredGram,
                    AvailableGram = available,
                    ConsumedGram = Math.Min(req.RequiredGram, available),
                    IsOverConsumed = req.RequiredGram > available
                });
            }

            schedule.IsInventoryDeducted = true;

            await _context.SaveChangesAsync(ct);
            await tx.CommitAsync(ct);

            return results;
        }catch
        {
            await tx.RollbackAsync(ct);
            throw;
        }
    }
}

