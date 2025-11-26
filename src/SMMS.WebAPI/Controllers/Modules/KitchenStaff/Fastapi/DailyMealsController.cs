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

[Route("fastapi/DailyMeals")]
[ApiController]
public class DailyMealsController : ControllerBase
{
    private readonly EduMealContext _context;

    public DailyMealsController(EduMealContext context)
    {
        _context = context;
    }

    // GET: api/DailyMeals
    [HttpGet]
    public async Task<ActionResult<IEnumerable<DailyMeal>>> GetDailyMeals()
    {
        return await _context.DailyMeals.ToListAsync();
    }

    // GET: api/DailyMeals/5
    [HttpGet("{id}")]
    public async Task<ActionResult<DailyMeal>> GetDailyMeal(int id)
    {
        var dailyMeal = await _context.DailyMeals.FindAsync(id);

        if (dailyMeal == null)
        {
            return NotFound();
        }

        return dailyMeal;
    }

    // PUT: api/DailyMeals/5
    // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
    [HttpPut("{id}")]
    public async Task<IActionResult> PutDailyMeal(int id, DailyMeal dailyMeal)
    {
        if (id != dailyMeal.DailyMealId)
        {
            return BadRequest();
        }

        _context.Entry(dailyMeal).State = EntityState.Modified;

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!DailyMealExists(id))
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

    // POST: api/DailyMeals
    // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
    [HttpPost]
    public async Task<ActionResult<DailyMeal>> PostDailyMeal([FromBody] CreateDailyMealDto dto)
    {
        var entity = new DailyMeal
        {
            ScheduleMealId = dto.ScheduleMealId,
            MealDate = dto.MealDate,
            MealType = dto.MealType,
            Notes = dto.Notes
        };

        _context.DailyMeals.Add(entity);
        await _context.SaveChangesAsync();
        return CreatedAtAction(nameof(GetDailyMeal), new { id = entity.DailyMealId }, entity);
    }


    // DELETE: api/DailyMeals/5
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteDailyMeal(int id)
    {
        var dailyMeal = await _context.DailyMeals.FindAsync(id);
        if (dailyMeal == null)
        {
            return NotFound();
        }

        _context.DailyMeals.Remove(dailyMeal);
        await _context.SaveChangesAsync();

        return NoContent();
    }

    private bool DailyMealExists(int id)
    {
        return _context.DailyMeals.Any(e => e.DailyMealId == id);
    }
}
