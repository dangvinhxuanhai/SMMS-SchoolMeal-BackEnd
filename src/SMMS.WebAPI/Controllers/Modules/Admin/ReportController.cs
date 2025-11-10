using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SMMS.Application.Features.auth.DTOs;
using SMMS.Application.Features.auth.Interfaces;

namespace SMMS.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Admin")]
    public class ReportController : ControllerBase
    {
        private readonly IReportRepository _reportRepo;

        public ReportController(IReportRepository reportRepo)
        {
            _reportRepo = reportRepo;
        }

        [HttpPost("users")]
        public async Task<IActionResult> GetUserReport([FromBody] ReportFilterDto filter)
        {
            var result = await _reportRepo.GetUserReportAsync(filter);
            return Ok(result);
        }
        [HttpGet("users")]
        public async Task<IActionResult> GetAllUserReport()
        {
            var result = await _reportRepo.GetAllUserReportAsync();
            return Ok(result);
        }
    }
}
