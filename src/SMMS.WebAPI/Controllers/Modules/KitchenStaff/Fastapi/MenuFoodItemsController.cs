using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SMMS.Domain.Entities.foodmenu;
using SMMS.Persistence.Data;
using SMMS.WebAPI.Controllers.Modules.KitchenStaff.Fastapi.DTOs;

namespace SMMS.WebAPI.Controllers.Modules.KitchenStaff.Fastapi;

[Route("fastapi/MenuFoodItems")]
[ApiController]
public class MenuFoodItemsController : ControllerBase
{
    private readonly EduMealContext _context;

    public MenuFoodItemsController(EduMealContext context)
    {
        _context = context;
    }

    // GET: api/MenuFoodItems
    [HttpGet]
    public async Task<ActionResult<IEnumerable<MenuFoodItem>>> GetMenuFoodItems()
    {
        return await _context.MenuFoodItems.ToListAsync();
    }

    // GET: api/MenuFoodItems/5
    [HttpGet("{id}")]
    public async Task<ActionResult<MenuFoodItem>> GetMenuFoodItem(int id)
    {
        var menuFoodItem = await _context.MenuFoodItems.FindAsync(id);

        if (menuFoodItem == null)
        {
            return NotFound();
        }

        return menuFoodItem;
    }

    // PUT: api/MenuFoodItems/5
    // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
    [HttpPut("{id}")]
    public async Task<IActionResult> PutMenuFoodItem(int id, MenuFoodItem menuFoodItem)
    {
        if (id != menuFoodItem.DailyMealId)
        {
            return BadRequest();
        }

        _context.Entry(menuFoodItem).State = EntityState.Modified;

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!MenuFoodItemExists(id))
            {
                return NotFound();
            }
            else
            {
                throw;
            }
        }

        return NoContent();
    }

    // POST: api/MenuFoodItems
    // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
    [HttpPost]
    public async Task<ActionResult<MenuFoodItem>> PostMenuFoodItem([FromBody] CreateMenuFoodItemDto dto)
    {
        var entity = new MenuFoodItem
        {
            DailyMealId = dto.DailyMealId,
            FoodId = dto.FoodId,
            SortOrder = dto.SortOrder
        };

        _context.MenuFoodItems.Add(entity);
        await _context.SaveChangesAsync();
        return CreatedAtAction(nameof(GetMenuFoodItem), new { id = entity.DailyMealId }, entity);
    }


    // DELETE: api/MenuFoodItems/5
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteMenuFoodItem(int id)
    {
        var menuFoodItem = await _context.MenuFoodItems.FindAsync(id);
        if (menuFoodItem == null)
        {
            return NotFound();
        }

        _context.MenuFoodItems.Remove(menuFoodItem);
        await _context.SaveChangesAsync();

        return NoContent();
    }

    private bool MenuFoodItemExists(int id)
    {
        return _context.MenuFoodItems.Any(e => e.DailyMealId == id);
    }
}
