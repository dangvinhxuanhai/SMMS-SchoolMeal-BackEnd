using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Query;
using SMMS.Application.Features.school.Interfaces;
using SMMS.Domain.Entities.school;
using SMMS.Application.Features.school.DTOs;
using Microsoft.AspNetCore.Authorization;
using MediatR;
using SMMS.Application.Features.school.Commands;
using SMMS.Application.Features.school.Queries;
using System.Security.Claims;

namespace SMMS.WebAPI.Controllers.Modules.Admin
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Admin")]
    public class SchoolsControllerA : ControllerBase
    {
        private readonly IMediator _mediator;

        public SchoolsControllerA(IMediator mediator)
        {
            _mediator = mediator;
        }
        private Guid GetCurrentUserId()
        {
            var idClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!Guid.TryParse(idClaim, out var adminId))
                throw new UnauthorizedAccessException("Token không hợp lệ.");
            return adminId;
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
        public async Task<IActionResult> Create([FromBody] CreateSchoolDto dto)
        {
            var userId = GetCurrentUserId();
            var schoolId = await _mediator.Send(new CreateSchoolCommand(dto, userId));
            return CreatedAtAction(nameof(GetById), new { id = schoolId }, new { id = schoolId });
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] UpdateSchoolDto dto)
        {
            var userId = GetCurrentUserId();
            await _mediator.Send(new UpdateSchoolCommand(id, dto, userId));
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            try
            {
                await _mediator.Send(new DeleteSchoolCommand(id));
                return NoContent();
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}
