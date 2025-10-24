using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SMMS.Application.Features.Manager.DTOs;
using SMMS.Application.Features.Manager.Interfaces;

namespace SMMS.WebAPI.Controllers.Modules.Manager;
[Route("api/[controller]")]
[ApiController]
public class ManagerHomeController : ControllerBase
{
    private readonly IManagerService _managerService;

    public ManagerHomeController(IManagerService managerService)
    {
        _managerService = managerService;
    }

    // 🟢 1. Dashboard tổng quan
    [HttpGet("overview")]
    public async Task<ActionResult<ManagerOverviewDto>> GetOverview([FromQuery] Guid schoolId)
    {
        if (schoolId == Guid.Empty)
            return BadRequest("schoolId không hợp lệ.");

        var result = await _managerService.GetOverviewAsync(schoolId);
        return Ok(result);
    }

    // 🟡 2. Các đơn mua hàng gần đây
    [HttpGet("recent-purchases")]
    public async Task<ActionResult<List<RecentPurchaseDto>>> GetRecentPurchases(
        [FromQuery] Guid schoolId,
        [FromQuery] int take = 8)
    {
        if (schoolId == Guid.Empty)
            return BadRequest("schoolId không hợp lệ.");

        var result = await _managerService.GetRecentPurchasesAsync(schoolId, take);
        return Ok(result);
    }
    // 🔴 Chi tiết đơn mua hàng
    [HttpGet("purchase-order/{orderId}/details")]
    public async Task<IActionResult> GetPurchaseOrderDetails(int orderId)
    {
        var result = await _managerService.GetPurchaseOrderDetailsAsync(orderId);
        return Ok(result);
    }
    // 🔵 3. Biểu đồ doanh thu (Revenue)
    [HttpGet("revenue")]
    public async Task<ActionResult<RevenueSeriesDto>> GetRevenue(
        [FromQuery] Guid schoolId,
        [FromQuery] DateTime from,
        [FromQuery] DateTime to,
        [FromQuery] string granularity = "daily")
    {
        if (schoolId == Guid.Empty)
            return BadRequest("schoolId không hợp lệ.");

        if (from >= to)
            return BadRequest("Khoảng thời gian không hợp lệ.");

        var result = await _managerService.GetRevenueAsync(schoolId, from, to, granularity);
        return Ok(result);
    }
}
