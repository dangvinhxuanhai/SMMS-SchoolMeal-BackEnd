using System;
using System.Collections.Generic;
using System.Linq;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SMMS.Application.Features.Menus.Command.MenuDayFoodItemz;
using SMMS.Application.Features.Menus.DTOs.MenuDayFoodItemz;
using SMMS.Application.Features.Menus.Queries.MenuDayFoodItemz;
using SMMS.Domain.Entities.foodmenu;

namespace SMMS.WebAPI.Controllers.Modules.KitchenStaff.v1.MenuDayFoodItemsManage;

[Route("api/v1/[controller]")]
[ApiController]
public class MenuDayFoodItemsController : ControllerBase
{
    private readonly ISender _sender;
    public MenuDayFoodItemsController(ISender sender) => _sender = sender;


    // GET: api/MenuFoodItems
    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken ct)
    {
        var result = await _sender.Send(new GetAllMenuDayFoodItemsQuery(), ct);
        return Ok(result);
    }

    // GET: api/MenuFoodItems/5
    [HttpGet("{menuDayId:int}/{foodId:int}")]
    public async Task<IActionResult> GetById(int menuDayId, int foodId, CancellationToken ct)
    {
        var result = await _sender.Send(new GetMenuDayFoodItemByIdQuery(menuDayId, foodId), ct);
        return result is null
            ? NotFound(Problem(title: $"MenuDayFoodItem ({menuDayId},{foodId}) not found"))
            : Ok(result);
    }

    // PUT: api/MenuFoodItems/5
    // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
    [HttpPut("{menuDayId:int}/{foodId:int}")]
    public async Task<IActionResult> Update(int menuDayId, int foodId, [FromBody] UpdateMenuDayFoodItemDto dto, CancellationToken ct)
    {
        var ok = await _sender.Send(new UpdateMenuDayFoodItemCommand(menuDayId, foodId, dto), ct);
        return ok
            ? Ok("UPDATE")
            : NotFound(Problem(title: $"MenuDayFoodItem ({menuDayId},{foodId}) not found"));
    }

    // POST: api/MenuFoodItems
    // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateMenuDayFoodItemDto dto, CancellationToken ct)
    {
        var keys = await _sender.Send(new CreateMenuDayFoodItemCommand(dto), ct);
        return CreatedAtAction(nameof(GetById), new { menuDayId = keys.MenuDayId, foodId = keys.FoodId }, keys);
    }


    // DELETE: api/MenuFoodItems/5
    [HttpDelete("{menuDayId:int}/{foodId:int}")]
    public async Task<IActionResult> Delete(int menuDayId, int foodId, CancellationToken ct)
    {
        var ok = await _sender.Send(new DeleteMenuDayFoodItemCommand(menuDayId, foodId), ct);
        return ok
            ? NoContent()
            : NotFound(Problem(title: $"MenuDayFoodItem ({menuDayId},{foodId}) not found"));
    }
}
