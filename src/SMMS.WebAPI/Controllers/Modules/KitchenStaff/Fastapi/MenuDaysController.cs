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

[Route("fastapi/MenuDays")]
[ApiController]
public class MenuDaysController : ControllerBase
{
    private readonly EduMealContext _context;

    public MenuDaysController(EduMealContext context)
    {
        _context = context;
    }

    // GET: api/MenuDays
    [HttpGet]
    public async Task<ActionResult<IEnumerable<MenuDay>>> GetMenuDays()
    {
        return await _context.MenuDays.ToListAsync();
    }

    // GET: api/MenuDays/5
    [HttpGet("{id}")]
    public async Task<ActionResult<MenuDay>> GetMenuDay(int id)
    {
        var menuDay = await _context.MenuDays.FindAsync(id);

        if (menuDay == null)
        {
            return NotFound();
        }

        return menuDay;
    }

    // PUT: api/MenuDays/5
    // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
    [HttpPut("{id}")]
    public async Task<IActionResult> PutMenuDay(int id, MenuDay menuDay)
    {
        if (id != menuDay.MenuDayId)
        {
            return BadRequest();
        }

        _context.Entry(menuDay).State = EntityState.Modified;

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!MenuDayExists(id))
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

    // POST: api/MenuDays
    // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
    [HttpPost]
    public async Task<ActionResult<MenuDay>> PostMenuDay([FromBody] CreateMenuDayDto dto)
    {
        var entity = new MenuDay
        {
            MenuId = dto.MenuId,
            DayOfWeek = dto.DayOfWeek,
            MealType = dto.MealType,
            Notes = dto.Notes
        };

        _context.MenuDays.Add(entity);
        await _context.SaveChangesAsync();
        return CreatedAtAction(nameof(GetMenuDay), new { id = entity.MenuDayId }, entity);
    }


    // DELETE: api/MenuDays/5
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteMenuDay(int id)
    {
        var menuDay = await _context.MenuDays.FindAsync(id);
        if (menuDay == null)
        {
            return NotFound();
        }

        _context.MenuDays.Remove(menuDay);
        await _context.SaveChangesAsync();

        return NoContent();
    }

    private bool MenuDayExists(int id)
    {
        return _context.MenuDays.Any(e => e.MenuDayId == id);
    }
}
