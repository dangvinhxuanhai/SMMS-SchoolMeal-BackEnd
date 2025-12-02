using MediatR;
using Microsoft.AspNetCore.Mvc;
using SMMS.Application.Features.Menus.Command.Menuing;
using SMMS.Application.Features.Menus.DTOs.Menuing;
using SMMS.Application.Features.Menus.Queries.Menuing;

namespace SMMS.WebAPI.Controllers.Modules.KitchenStaff.v1.MenuManage;

[Route("api/v1/[controller]")]
[ApiController]
public class MenusController : ControllerBase
{
    private readonly ISender _sender;
    public MenusController(ISender sender) => _sender = sender;


    // GET: api/Menus
    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken ct)
    {
        var result = await _sender.Send(new GetAllMenusQuery(), ct);
        return Ok(result);
    }

    // GET: api/Menus/5
    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id, CancellationToken ct)
    {
        var result = await _sender.Send(new GetMenuByIdQuery(id), ct);
        return result is null
            ? NotFound(Problem(title: $"Menu {id} not found"))
            : Ok(result);
    }

    // PUT: api/Menus/5
    // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateMenuDto dto, CancellationToken ct)
    {
        var ok = await _sender.Send(new UpdateMenuCommand(id, dto), ct);
        return ok
            ? Ok("UPDATE")
            : NotFound(Problem(title: $"Menu {id} not found"));
    }

    // POST: api/Menus
    // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateMenuDto dto, CancellationToken ct)
    {
        var id = await _sender.Send(new CreateMenuCommand(dto), ct);
        return CreatedAtAction(nameof(GetById), new { id }, new { id });
    }


    // DELETE: api/Menus/5
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        var ok = await _sender.Send(new DeleteMenuCommand(id), ct);
        return ok
            ? NoContent()
            : NotFound(Problem(title: $"Menu {id} not found"));
    }
}
