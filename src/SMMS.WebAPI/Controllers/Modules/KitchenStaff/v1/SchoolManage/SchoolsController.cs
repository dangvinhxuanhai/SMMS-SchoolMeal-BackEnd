using MediatR;
using Microsoft.AspNetCore.Mvc;
using SMMS.Application.Features.Menus.Command.Schooling;
using SMMS.Application.Features.Menus.DTOs.Schooling;
using SMMS.Application.Features.Menus.Queries.Schooling;

namespace SMMS.WebAPI.Controllers.Modules.KitchenStaff.v1.SchoolManage;

[Route("api/v1/[controller]")]
[ApiController]
public class SchoolsController : ControllerBase
{
    private readonly ISender _sender;

    public SchoolsController(ISender sender) => _sender = sender;

    // GET: api/Schools
    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken ct)
    {
        var result = await _sender.Send(new GetAllSchoolsQuery(), ct);
        return Ok(result);
    }

    // GET: api/Schools/5
    [HttpGet("{id:guid}", Name = "School_GetById")]
    public async Task<IActionResult> GetById([FromRoute] Guid id, CancellationToken ct)
    {
        var data = await _sender.Send(new GetSchoolByIdQuery(id), ct);
        return data is null ? NotFound() : Ok(data);
    }

    // PUT: api/Schools/5
    // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateSchoolDto dto, CancellationToken ct)
    {
        var success = await _sender.Send(new UpdateSchoolCommand(id, dto), ct);
        return success
            ? Ok("UPDATE")
            : NotFound(Problem(title: $"School {id} not found"));
    }

    // POST: api/Schools
    // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateSchoolDto dto, CancellationToken ct)
    {
        var id = await _sender.Send(new CreateSchoolCommand(dto), ct);
        return CreatedAtAction(nameof(GetById), new { id }, new { id });
    }

    // DELETE: api/Schools/5
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        var success = await _sender.Send(new DeleteSchoolCommand(id), ct);
        return success
            ? NoContent()
            : NotFound(Problem(title: $"School {id} not found"));
    }
}
