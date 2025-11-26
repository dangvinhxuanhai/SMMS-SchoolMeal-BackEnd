using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SMMS.Application.Features.Manager.DTOs;
using SMMS.Application.Features.Manager.Interfaces;
using SMMS.Application.Features.Manager.Queries;

namespace SMMS.WebAPI.Controllers.Modules.Manager;
[Route("api/[controller]")]
[ApiController]
[Authorize(Roles = "Manager")]
public class ManagerHomeController : ControllerBase
{
    private readonly IMediator _mediator;

    public ManagerHomeController(IMediator mediator)
    {
        _mediator = mediator;
    }
    private Guid GetSchoolIdFromToken()
    {
        var schoolIdClaim = User.FindFirst("SchoolId")?.Value;
        if (string.IsNullOrEmpty(schoolIdClaim))
            throw new UnauthorizedAccessException("Không tìm thấy SchoolId trong token.");

        return Guid.Parse(schoolIdClaim);
    }
    // 🟢 1. Dashboard tổng quan
    // GET: /api/ManagerHome/overview?schoolId=...
    [HttpGet("overview")]
    public async Task<ActionResult<ManagerOverviewDto>> GetOverview()
    {
        var schoolId = GetSchoolIdFromToken();
        if (schoolId == Guid.Empty)
            return BadRequest("schoolId không hợp lệ.");

        var result = await _mediator.Send(new GetManagerOverviewQuery(schoolId));
        return Ok(result);
    }

    // 🟡 2. Các đơn mua hàng gần đây
    // GET: /api/ManagerHome/recent-purchases?schoolId=...&take=8
    [HttpGet("recent-purchases")]
    public async Task<ActionResult<List<RecentPurchaseDto>>> GetRecentPurchases(
        [FromQuery] int take = 8)
    {
        var schoolId = GetSchoolIdFromToken();
        if (schoolId == Guid.Empty)
            return BadRequest("schoolId không hợp lệ.");

        var result = await _mediator.Send(new GetRecentPurchasesQuery(schoolId, take));
        return Ok(result);
    }

    // 🔴 Chi tiết đơn mua hàng
    // GET: /api/ManagerHome/purchase-order/{orderId}/details
    [HttpGet("purchase-order/{orderId:int}/details")]
    public async Task<IActionResult> GetPurchaseOrderDetails(int orderId)
    {
        var result = await _mediator.Send(new GetPurchaseOrderDetailsQuery(orderId));
        return Ok(result);
    }

    // 🔵 3. Biểu đồ doanh thu (Revenue)
    // GET: /api/ManagerHome/revenue?schoolId=...&from=...&to=...&granularity=daily
    [HttpGet("revenue")]
    public async Task<ActionResult<RevenueSeriesDto>> GetRevenue(
        [FromQuery] DateTime from,
        [FromQuery] DateTime to,
        [FromQuery] string granularity = "daily")
    {
        var schoolId = GetSchoolIdFromToken();
        if (schoolId == Guid.Empty)
            return BadRequest("schoolId không hợp lệ.");

        if (from >= to)
            return BadRequest("Khoảng thời gian không hợp lệ.");

        var result = await _mediator.Send(
            new GetRevenueQuery(schoolId, from, to, granularity)
        );

        return Ok(result);
    }
}
