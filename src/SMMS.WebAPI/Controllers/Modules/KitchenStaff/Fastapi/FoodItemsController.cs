using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SMMS.Domain.Entities.nutrition;
using SMMS.Persistence.Data;
using SMMS.WebAPI.Controllers.Modules.KitchenStaff.Fastapi.DTOs;

namespace SMMS.WebAPI.Controllers.Modules.KitchenStaff.Fastapi;

[Route("fastapi/FoodItems")]
[ApiController]
public class FoodItemsController : ControllerBase
{
    private readonly EduMealContext _context;

    public FoodItemsController(EduMealContext context)
    {
        _context = context;
    }

    // GET: api/FoodItems
    [HttpGet]
    public async Task<ActionResult<IEnumerable<FoodItem>>> GetFoodItems()
    {
        return await _context.FoodItems.ToListAsync();
    }

    // GET: api/FoodItems/5
    [HttpGet("{id}")]
    public async Task<ActionResult<FoodItem>> GetFoodItem(int id)
    {
        var foodItem = await _context.FoodItems.FindAsync(id);

        if (foodItem == null)
        {
            return NotFound();
        }

        return foodItem;
    }

    // PUT: api/FoodItems/5
    // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
    [HttpPut("{id}")]
    public async Task<IActionResult> PutFoodItem(int id, FoodItem foodItem)
    {
        if (id != foodItem.FoodId)
        {
            return BadRequest();
        }

        _context.Entry(foodItem).State = EntityState.Modified;

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!FoodItemExists(id))
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

    // POST: api/FoodItems
    // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
    [HttpPost]
    public async Task<ActionResult<FoodItem>> PostFoodItem([FromBody] CreateFoodItemDto dto)
    {
        var food = new FoodItem
        {
            FoodName = dto.FoodName,
            FoodType = dto.FoodType,
            FoodDesc = dto.FoodDesc,
            ImageUrl = dto.ImageUrl,
            CreatedBy = dto.CreatedBy,
            CreatedAt = DateTime.UtcNow,
            SchoolId = dto.SchoolId,
            IsActive = dto.IsActive
        };

        _context.FoodItems.Add(food);
        await _context.SaveChangesAsync();
        return CreatedAtAction(nameof(GetFoodItem), new { id = food.FoodId }, food);
    }


    // DELETE: api/FoodItems/5
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteFoodItem(int id)
    {
        var foodItem = await _context.FoodItems.FindAsync(id);
        if (foodItem == null)
        {
            return NotFound();
        }

        _context.FoodItems.Remove(foodItem);
        await _context.SaveChangesAsync();

        return NoContent();
    }

    private bool FoodItemExists(int id)
    {
        return _context.FoodItems.Any(e => e.FoodId == id);
    }
}
