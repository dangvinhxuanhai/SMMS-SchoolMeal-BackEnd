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
    // Đã sửa: Thêm .Include(x => x.Food) để lấy tên món ăn
    [HttpGet]
    public async Task<ActionResult<IEnumerable<MenuDayFoodItem>>> GetMenuDayFoodItems()
    {
        return await _context.MenuDayFoodItems
            .Include(x => x.Food) // Quan trọng: Lấy thông tin bảng Food
            .ToListAsync();
    }

    // GET: api/MenuDayFoodItems/5/12
    [HttpGet("{menuDayId}/{foodId}")]
    public async Task<ActionResult<MenuDayFoodItem>> GetMenuDayFoodItem(int menuDayId, int foodId)
    {
        var menuDayFoodItem = await _context.MenuDayFoodItems
            .Include(x => x.Food)
            .FirstOrDefaultAsync(x => x.MenuDayId == menuDayId && x.FoodId == foodId);

        if (menuDayFoodItem == null)
        {
            return NotFound();
        }

        return menuDayFoodItem;
    }

    // PUT: api/MenuDayFoodItems/5/12
    [HttpPut("{menuDayId}/{foodId}")]
    public async Task<IActionResult> PutMenuDayFoodItem(int menuDayId, int foodId, MenuDayFoodItem menuDayFoodItem)
    {
        if (menuDayId != menuDayFoodItem.MenuDayId || foodId != menuDayFoodItem.FoodId)
        {
            return BadRequest("ID mismatch");
        }

        _context.Entry(menuDayFoodItem).State = EntityState.Modified;

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!MenuDayFoodItemExists(menuDayId, foodId))
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
    [HttpPost]
    public async Task<ActionResult<MenuDayFoodItem>> PostMenuDayFoodItem([FromBody] CreateMenuDayFoodItemDto dto)
    {
        var entity = new MenuDayFoodItem
        {
            MenuDayId = dto.MenuDayId,
            FoodId = dto.FoodId,
            SortOrder = dto.SortOrder
        };

        // Kiểm tra xem đã tồn tại chưa để tránh lỗi duplicate key
        if (MenuDayFoodItemExists(entity.MenuDayId, entity.FoodId))
        {
            return Conflict("Item already exists in this menu day");
        }

        _context.MenuDayFoodItems.Add(entity);
        await _context.SaveChangesAsync();

        // Load lại entity kèm Food info để trả về cho FE hiển thị ngay lập tức
        var createdEntity = await _context.MenuDayFoodItems
            .Include(x => x.Food)
            .FirstOrDefaultAsync(x => x.MenuDayId == entity.MenuDayId && x.FoodId == entity.FoodId);

        return CreatedAtAction(nameof(GetMenuDayFoodItem), new { menuDayId = entity.MenuDayId, foodId = entity.FoodId }, createdEntity);
    }


    // DELETE: api/MenuDayFoodItems/5/12
    [HttpDelete("{menuDayId}/{foodId}")]
    public async Task<IActionResult> DeleteMenuDayFoodItem(int menuDayId, int foodId)
    {
        var menuDayFoodItem = await _context.MenuDayFoodItems
            .FirstOrDefaultAsync(x => x.MenuDayId == menuDayId && x.FoodId == foodId);

        if (menuDayFoodItem == null)
        {
            return NotFound();
        }

        _context.MenuDayFoodItems.Remove(menuDayFoodItem);
        await _context.SaveChangesAsync();

        return NoContent();
    }

    private bool MenuDayFoodItemExists(int menuDayId, int foodId)
    {
        return _context.MenuDayFoodItems.Any(e => e.MenuDayId == menuDayId && e.FoodId == foodId);
    }
}
