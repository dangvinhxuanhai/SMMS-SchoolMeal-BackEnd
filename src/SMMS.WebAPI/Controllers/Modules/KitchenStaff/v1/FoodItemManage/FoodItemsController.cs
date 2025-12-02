using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SMMS.Application.Features.Menus.Command;
using SMMS.Application.Features.Menus.DTOs;
using SMMS.Application.Features.Menus.Queries;
using SMMS.Domain.Entities.nutrition;
using SMMS.Persistence.Data;

namespace SMMS.WebAPI.Controllers.Modules.KitchenStaff.v1.FoodItemManage;

[Route("api/v1/[controller]")]
[ApiController]
public class FoodItemsController : ControllerBase
{
    private readonly ISender _sender;

    public FoodItemsController(ISender sender)
    {
        _sender = sender;
    }


    // GET: api/FoodItems
    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken ct)
    {
        var result = await _sender.Send(new GetAllFoodItemsQuery(), ct);
        return Ok(result);
    }

    // GET: api/FoodItems/5
    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id, CancellationToken ct)
    {
        var result = await _sender.Send(new GetFoodItemByIdQuery(id), ct);
        return result is null ? NotFound(Problem(title: $"FoodItem {id} not found")) : Ok(result);
    }

    // PUT: api/FoodItems/5
    // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateFoodItemDto dto, CancellationToken ct)
    {
        var success = await _sender.Send(new UpdateFoodItemCommand(id, dto), ct);
        return success ? Ok("UPDATE") : NotFound(Problem(title: $"FoodItem {id} not found"));
    }

    // POST: api/FoodItems
    // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateFoodItemDto dto, CancellationToken ct)
    {
        var id = await _sender.Send(new CreateFoodItemCommand(dto), ct);
        return CreatedAtAction(nameof(GetById), new { id }, new { id });
    }


    // DELETE: api/FoodItems/5
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        var success = await _sender.Send(new DeleteFoodItemCommand(id), ct);
        return success ? NoContent() : NotFound(Problem(title: $"FoodItem {id} not found"));
    }

    /*private bool FoodItemExists(int id)
    {
        return _context.FoodItems.Any(e => e.FoodId == id);
    }*/
}
