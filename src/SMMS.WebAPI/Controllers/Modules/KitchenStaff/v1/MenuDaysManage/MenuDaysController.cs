using System;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using SMMS.Application.Features.Menus.Command.MenuDaying;
using SMMS.Application.Features.Menus.DTOs.MenuDaying;
using SMMS.Application.Features.Menus.Queries.MenuDaying;
using SMMS.Domain.Entities.foodmenu;

namespace SMMS.WebAPI.Controllers.Modules.KitchenStaff.v1.MenuDaysManage;

[Route("api/v1/[controller]")]
[ApiController]
public class MenuDaysController : ControllerBase
{
    private readonly ISender _sender;
    public MenuDaysController(ISender sender) => _sender = sender;


    // GET: api/MenuDays
    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken ct)
    {
        var result = await _sender.Send(new GetAllMenuDaysQuery(), ct);
        return Ok(result);
    }

    // GET: api/MenuDays/5
    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id, CancellationToken ct)
    {
        var result = await _sender.Send(new GetMenuDayByIdQuery(id), ct);
        return result is null
            ? NotFound(Problem(title: $"MenuDay {id} not found"))
            : Ok(result);
    }

    // PUT: api/MenuDays/5
    // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateMenuDayDto dto, CancellationToken ct)
    {
        var ok = await _sender.Send(new UpdateMenuDayCommand(id, dto), ct);
        return ok
            ? Ok("UPDATE")
            : NotFound(Problem(title: $"MenuDay {id} not found"));
    }


    // POST: api/MenuDays
    // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateMenuDayDto dto, CancellationToken ct)
    {
        var id = await _sender.Send(new CreateMenuDayCommand(dto), ct);
        return CreatedAtAction(nameof(GetById), new { id }, new { id });
    }


    // DELETE: api/MenuDays/5
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        var ok = await _sender.Send(new DeleteMenuDayCommand(id), ct);
        return ok
            ? NoContent()
            : NotFound(Problem(title: $"MenuDay {id} not found"));
    }
}
