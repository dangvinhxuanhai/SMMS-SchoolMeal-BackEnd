using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SMMS.Application.Features.auth.DTOs;
using SMMS.Application.Features.auth.Interfaces;
using SMMS.Application.Features.auth.Queries;

namespace SMMS.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Admin")]
    public class ReportController : ControllerBase
    {
        private readonly IMediator _mediator;

        public ReportController(IMediator mediator)
        {
            _mediator = mediator;
        }

        // ✅ API: Lấy báo cáo có bộ lọc
        [HttpPost("users")]
        public async Task<IActionResult> GetUserReport([FromBody] ReportFilterDto filter)
        {
            var result = await _mediator.Send(new GetUserReportQuery(filter));
            return Ok(result);
        }

        // ✅ API: Lấy toàn bộ báo cáo người dùng
        [HttpGet("users")]
        public async Task<IActionResult> GetAllUserReport()
        {
            var result = await _mediator.Send(new GetAllUserReportQuery());
            return Ok(result);
        }
    }
}
