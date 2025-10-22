using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SMMS.Application.Features.school.Interfaces;

namespace SMMS.WebAPI.Controllers.Modules.Admin
{
    [Authorize(Roles = "Admin")]
    [ApiController]
    [Route("api/admin/dashboard")]
    public class DashboardController : ControllerBase
    {
        private readonly IAdminDashboardRepository _dashboardRepository;

        public DashboardController(IAdminDashboardRepository dashboardRepository)
        {
            _dashboardRepository = dashboardRepository;
        }

        [HttpGet("overview")]
        public async Task<IActionResult> GetOverview()
        {
            var result = await _dashboardRepository.GetSystemOverviewAsync();
            return Ok(result);
        }
    }
}
