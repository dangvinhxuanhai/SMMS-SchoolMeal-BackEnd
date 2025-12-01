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
}
