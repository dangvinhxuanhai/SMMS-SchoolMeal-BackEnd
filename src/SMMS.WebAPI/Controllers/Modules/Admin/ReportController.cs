using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SMMS.Application.Features.auth.DTOs;
using SMMS.Application.Features.auth.Interfaces;
using SMMS.Application.Features.auth.Queries;

namespace SMMS.WebAPI.Controllers.Modules.Admin
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
        [HttpGet("Finance")]
        public async Task<IActionResult> GetAllFinance()
        {
            var result = await _mediator.Send(new GetAllFinanceReportQuery());
            return Ok(result);
        }
        [HttpPost("Finance")]
        public async Task<IActionResult> GetFinanceReport([FromBody] FinanceReportFilterDto filter)
        {
            var result = await _mediator.Send(new GetFinanceReportQuery(filter));
            return Ok(result);

        }
    }
}
