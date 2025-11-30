using System.Security.Claims;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SMMS.Application.Features.billing.Commands;
using SMMS.Application.Features.billing.DTOs;
using SMMS.Application.Features.billing.Queries;

namespace SMMS.WebAPI.Controllers.Modules.Admin
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Admin")]
    public class SchoolContactController : ControllerBase
    {
        private readonly IMediator _mediator;
        public SchoolContactController(IMediator mediator) => _mediator = mediator;

        private Guid GetCurrentUserId()
        {
            var idClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!Guid.TryParse(idClaim, out var adminId))
                throw new UnauthorizedAccessException("Token không hợp lệ.");
            return adminId;
        }
        [HttpGet]
        public async Task<IActionResult> GetBySchool([FromQuery] Guid schoolId) =>
            Ok(await _mediator.Send(new GetRevenuesBySchoolQuery(schoolId)));

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(long id)
        {
            var revenue = await _mediator.Send(new GetRevenueByIdQuery(id));
            if (revenue == null) return NotFound();
            return Ok(revenue);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromForm] CreateSchoolRevenueDto dto)
        {
            var userId = GetCurrentUserId();
            var revenueId = await _mediator.Send(new CreateSchoolRevenueCommand(dto, userId));
            return CreatedAtAction(nameof(GetById), new { id = revenueId }, null);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(long id, [FromForm] UpdateSchoolRevenueDto dto)
        {
            var userId = GetCurrentUserId();
            await _mediator.Send(new UpdateSchoolRevenueCommand(id, dto, userId));
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(long id)
        {
            await _mediator.Send(new DeleteSchoolRevenueCommand(id));
            return NoContent();
        }
    }
}
