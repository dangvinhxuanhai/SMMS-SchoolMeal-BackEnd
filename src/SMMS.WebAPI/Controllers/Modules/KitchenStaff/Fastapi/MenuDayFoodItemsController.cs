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

[Route("fastapi/MenuDayFoodItems")]
[ApiController]
public class MenuDayFoodItemsController : ControllerBase
{
    private readonly EduMealContext _context;

    public MenuDayFoodItemsController(EduMealContext context)
    {
        _context = context;
    }

    // GET: api/MenuDayFoodItems
    [HttpGet]
    public async Task<ActionResult<IEnumerable<MenuDayFoodItem>>> GetMenuDayFoodItems()
    {
        return await _context.MenuDayFoodItems.ToListAsync();
    }

    // GET: api/MenuDayFoodItems/5
    [HttpGet("{id}")]
    public async Task<ActionResult<MenuDayFoodItem>> GetMenuDayFoodItem(int id)
    {
        var menuDayFoodItem = await _context.MenuDayFoodItems.FindAsync(id);

        if (menuDayFoodItem == null)
        {
            return NotFound();
        }

        return menuDayFoodItem;
    }

    // PUT: api/MenuDayFoodItems/5
    // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
    [HttpPut("{id}")]
    public async Task<IActionResult> PutMenuDayFoodItem(int id, MenuDayFoodItem menuDayFoodItem)
    {
        if (id != menuDayFoodItem.MenuDayId)
        {
            return BadRequest();
        }

        _context.Entry(menuDayFoodItem).State = EntityState.Modified;

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!MenuDayFoodItemExists(id))
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

    // POST: api/MenuDayFoodItems
    // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
    [HttpPost]
    public async Task<ActionResult<MenuDayFoodItem>> PostMenuDayFoodItem([FromBody] CreateMenuDayFoodItemDto dto)
    {
        var entity = new MenuDayFoodItem
        {
            MenuDayId = dto.MenuDayId,
            FoodId = dto.FoodId,
            SortOrder = dto.SortOrder
        };

        _context.MenuDayFoodItems.Add(entity);
        await _context.SaveChangesAsync();
        return CreatedAtAction(nameof(GetMenuDayFoodItem), new { id = entity.MenuDayId }, entity);
    }


    // DELETE: api/MenuDayFoodItems/5
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteMenuDayFoodItem(int id)
    {
        var menuDayFoodItem = await _context.MenuDayFoodItems.FindAsync(id);
        if (menuDayFoodItem == null)
        {
            return NotFound();
        }

        _context.MenuDayFoodItems.Remove(menuDayFoodItem);
        await _context.SaveChangesAsync();

        return NoContent();
    }

    private bool MenuDayFoodItemExists(int id)
    {
        return _context.MenuDayFoodItems.Any(e => e.MenuDayId == id);
    }
}
