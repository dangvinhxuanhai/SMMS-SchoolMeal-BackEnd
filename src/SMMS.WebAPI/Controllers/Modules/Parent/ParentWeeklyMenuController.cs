using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SMMS.Application.Features.foodmenu.DTOs;
using SMMS.Application.Features.foodmenu.Queries;

namespace SMMS.WebAPI.Controllers.Modules.Parent
{
    [Authorize(Roles = "Parent")]
    [ApiController]
    [Route("api/weekly-menu")]
    public class ParentWeeklyMenuController : ControllerBase
    {
        private readonly IMediator _mediator;

        public ParentWeeklyMenuController(IMediator mediator)
        {
            _mediator = mediator;
        }

        /// <summary>
        /// Lấy thực đơn tuần của một học sinh theo một ngày bất kỳ trong tuần đó.
        /// </summary>
        [HttpGet("week-menu")]
        [ProducesResponseType(typeof(WeekMenuDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetWeekMenu(
            [FromQuery] Guid studentId,
            [FromQuery] DateTime date)
        {
            if (studentId == Guid.Empty)
                return BadRequest("studentId is required.");

            if (date == default)
                return BadRequest("date is required.");

            var result = await _mediator.Send(new GetWeekMenuQuery(studentId, date));

            if (result is null)
                return NotFound("Không tìm thấy thực đơn tuần.");

            return Ok(result);
        }

        /// <summary>
        /// Lấy danh sách tuần có sẵn thực đơn cho học sinh.
        /// </summary>
        [HttpGet("available-weeks")]
        [ProducesResponseType(typeof(IReadOnlyList<WeekOptionDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAvailableWeeks(
            [FromQuery] Guid studentId,
            [FromQuery] DateTime? from,
            [FromQuery] DateTime? to)
        {
            if (studentId == Guid.Empty)
                return BadRequest("studentId is required.");

            var result = await _mediator.Send(new GetAvailableWeeksQuery(studentId, from, to));

            return Ok(result);
        }
    }
}
