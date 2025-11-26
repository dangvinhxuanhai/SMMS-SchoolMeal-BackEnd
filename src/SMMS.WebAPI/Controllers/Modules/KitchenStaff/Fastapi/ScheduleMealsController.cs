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

[Route("fastapi/ScheduleMeals")]
[ApiController]
public class ScheduleMealsController : ControllerBase
{
    private readonly EduMealContext _context;

    public ScheduleMealsController(EduMealContext context)
    {
        _context = context;
    }

    // GET: api/ScheduleMeals
    [HttpGet]
    public async Task<ActionResult<IEnumerable<ScheduleMeal>>> GetScheduleMeals()
    {
        return await _context.ScheduleMeals.ToListAsync();
    }

    // GET: api/ScheduleMeals/5
    [HttpGet("{id}")]
    public async Task<ActionResult<ScheduleMeal>> GetScheduleMeal(long id)
    {
        var scheduleMeal = await _context.ScheduleMeals.FindAsync(id);

        if (scheduleMeal == null)
        {
            return NotFound();
        }

        return scheduleMeal;
    }

    // PUT: api/ScheduleMeals/5
    // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
    [HttpPut("{id}")]
    public async Task<IActionResult> PutScheduleMeal(long id, ScheduleMeal scheduleMeal)
    {
        if (id != scheduleMeal.ScheduleMealId)
        {
            return BadRequest();
        }

        _context.Entry(scheduleMeal).State = EntityState.Modified;

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!ScheduleMealExists(id))
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

    // POST: api/ScheduleMeals
    // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
    [HttpPost]
    public async Task<ActionResult<ScheduleMeal>> PostScheduleMeal([FromBody] CreateScheduleMealDto dto)
    {
        var entity = new ScheduleMeal
        {
            SchoolId = dto.SchoolId,
            MenuId = dto.MenuId,
            WeekStart = dto.WeekStart ,
            WeekEnd = dto.WeekEnd ,
            WeekNo = dto.WeekNo,
            YearNo = dto.YearNo,
            Status = dto.Status,
            PublishedAt = dto.PublishedAt,
            Notes = dto.Notes,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = dto.CreatedBy
        };

        _context.ScheduleMeals.Add(entity);
        await _context.SaveChangesAsync();
        return CreatedAtAction(nameof(GetScheduleMeal), new { id = entity.ScheduleMealId }, entity);
    }


    // DELETE: api/ScheduleMeals/5
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteScheduleMeal(long id)
    {
        var scheduleMeal = await _context.ScheduleMeals.FindAsync(id);
        if (scheduleMeal == null)
        {
            return NotFound();
        }

        _context.ScheduleMeals.Remove(scheduleMeal);
        await _context.SaveChangesAsync();

        return NoContent();
    }

    private bool ScheduleMealExists(long id)
    {
        return _context.ScheduleMeals.Any(e => e.ScheduleMealId == id);
    }
}
