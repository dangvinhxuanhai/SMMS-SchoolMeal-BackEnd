using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Query;
using SMMS.Application.Features.school.Interfaces;
using SMMS.Domain.Entities.school;
using SMMS.Application.Features.school.DTOs;
using Microsoft.AspNetCore.Authorization;
using MediatR;
using SMMS.Application.Features.school.Commands;
using SMMS.Application.Features.school.Queries;

namespace SMMS.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Admin")]
    public class SchoolsController : ControllerBase
    {
        private readonly IMediator _mediator;

        public SchoolsController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [EnableQuery]
        [HttpGet]
        public async Task<IActionResult> Get()
        {
            var schools = await _mediator.Send(new GetAllSchoolsQuery());
            return Ok(schools);
        }

        [EnableQuery]
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var school = await _mediator.Send(new GetSchoolByIdQuery(id));
            if (school == null) return NotFound();
            return Ok(school);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromForm] CreateSchoolDto dto)
        {
            var schoolId = await _mediator.Send(new CreateSchoolCommand(dto));
            return CreatedAtAction(nameof(GetById), new { id = schoolId }, null);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(Guid id, [FromForm] UpdateSchoolDto dto)
        {
            await _mediator.Send(new UpdateSchoolCommand(id, dto));
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            await _mediator.Send(new DeleteSchoolCommand(id));
            return NoContent();
        }
    }
}
