using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SMMS.Application.Features.Meal.Interfaces;
using SMMS.Domain.Entities.foodmenu;
using SMMS.Persistence.Data;

namespace SMMS.Persistence.Repositories.Meal;
public class MenuRepository : IMenuRepository
{
    private readonly EduMealContext _context;

    public MenuRepository(EduMealContext context)
    {
        _context = context;
    }

    public async Task<Menu?> GetWithDetailsAsync(int menuId, CancellationToken ct = default)
    {
        return await _context.Menus
            .Include(m => m.MenuDays)
                .ThenInclude(d => d.MenuDayFoodItems)
            .FirstOrDefaultAsync(m => m.MenuId == menuId, ct);
    }

    public async Task AddAsync(Menu menu, CancellationToken ct = default)
    {
        await _context.Menus.AddAsync(menu, ct);
    }

    public async Task<List<Menu>> GetListBySchoolAsync(
       Guid schoolId,
       int? yearId,
       short? weekNo,
       CancellationToken ct = default)
    {
        var query = _context.Menus.AsQueryable();

        query = query.Where(m => m.SchoolId == schoolId && !m.AskToDelete);

        if (yearId.HasValue)
            query = query.Where(m => m.YearId == yearId.Value);

        if (weekNo.HasValue)
            query = query.Where(m => m.WeekNo == weekNo.Value);

        return await query
            .OrderByDescending(m => m.CreatedAt)
            .ToListAsync(ct);
    }

    public async Task<Menu?> GetDetailWithFoodAsync(
        int menuId,
        Guid schoolId,
        CancellationToken ct = default)
    {
        return await _context.Menus
            .Where(m => m.MenuId == menuId && m.SchoolId == schoolId)
            .Include(m => m.MenuDays)
                .ThenInclude(d => d.MenuDayFoodItems)
                    .ThenInclude(f => f.Food) // navigation tới FoodItem
            .FirstOrDefaultAsync(ct);
    }

    public async Task<bool> HardDeleteAsync(int menuId, CancellationToken cancellationToken = default)
    {
        var menu = await _context.Menus
            .FirstOrDefaultAsync(m => m.MenuId == menuId, cancellationToken);

        if (menu == null)
            return false;

        using var tx = await _context.Database.BeginTransactionAsync(cancellationToken);

        // 1. Xóa FoodInFridge (IngredientInFridge sẽ cascade)
        var foodInFridges = await _context.FoodInFridges
            .Where(f => f.MenuId == menuId)
            .ToListAsync(cancellationToken);

        if (foodInFridges.Count > 0)
        {
            _context.FoodInFridges.RemoveRange(foodInFridges);
        }

        // 2. Xóa MenuDayFoodItems & MenuDays
        var menuDays = await _context.MenuDays
            .Where(md => md.MenuId == menuId)
            .ToListAsync(cancellationToken);

        if (menuDays.Count > 0)
        {
            var menuDayIds = menuDays.Select(md => md.MenuDayId).ToList();

            var menuDayFoodItems = await _context.MenuDayFoodItems
                .Where(mdf => menuDayIds.Contains(mdf.MenuDayId))
                .ToListAsync(cancellationToken);

            if (menuDayFoodItems.Count > 0)
            {
                _context.MenuDayFoodItems.RemoveRange(menuDayFoodItems);
            }

            _context.MenuDays.RemoveRange(menuDays);
        }

        // 3. Xóa Menu
        _context.Menus.Remove(menu);

        await _context.SaveChangesAsync(cancellationToken);
        await tx.CommitAsync(cancellationToken);

        return true;
    }
}
