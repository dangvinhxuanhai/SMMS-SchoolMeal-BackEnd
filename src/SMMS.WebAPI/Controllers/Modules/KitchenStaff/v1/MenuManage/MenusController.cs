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

namespace SMMS.WebAPI.Controllers.Modules.KitchenStaff.v1.MenuManage;

[Route("fastapi/Menus")]
[ApiController]
public class MenusController : ControllerBase
{
    private readonly EduMealContext _context;

    public MenusController(EduMealContext context)
    {
        _context = context;
    }

    // GET: api/Menus
    [HttpGet]
    public async Task<ActionResult<IEnumerable<Menu>>> GetMenus()
    {
        return await _context.Menus.ToListAsync();
    }

    // GET: api/Menus/5
    [HttpGet("{id}")]
    public async Task<ActionResult<Menu>> GetMenu(int id)
    {
        var menu = await _context.Menus.FindAsync(id);

        if (menu == null)
        {
            return NotFound();
        }

        return menu;
    }

    // PUT: api/Menus/5
    // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
    [HttpPut("{id}")]
    public async Task<IActionResult> PutMenu(int id, Menu menu)
    {
        if (id != menu.MenuId)
        {
            return BadRequest();
        }

        _context.Entry(menu).State = EntityState.Modified;

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!MenuExists(id))
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

    // POST: api/Menus
    // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
    [HttpPost]
    public async Task<ActionResult<Menu>> PostMenu([FromBody] CreateMenuDto dto)
    {
        var menu = new Menu
        {
            PublishedAt = dto.PublishedAt,
            SchoolId = dto.SchoolId,
            IsVisible = dto.IsVisible,
            WeekNo = dto.WeekNo,
            CreatedAt = DateTime.UtcNow,
            ConfirmedBy = dto.ConfirmedBy,
            ConfirmedAt = dto.ConfirmedAt,
            AskToDelete = dto.AskToDelete,
            YearId = dto.YearId
        };

        _context.Menus.Add(menu);
        await _context.SaveChangesAsync();
        return CreatedAtAction(nameof(GetMenu), new { id = menu.MenuId }, menu);
    }


    // DELETE: api/Menus/5
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteMenu(int id)
    {
        var menu = await _context.Menus.FindAsync(id);
        if (menu == null)
        {
            return NotFound();
        }

        _context.Menus.Remove(menu);
        await _context.SaveChangesAsync();

        return NoContent();
    }

    private bool MenuExists(int id)
    {
        return _context.Menus.Any(e => e.MenuId == id);
    }
}
