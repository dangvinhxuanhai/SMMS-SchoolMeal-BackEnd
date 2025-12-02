using System.Security.Claims;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using SMMS.Application.Features.foodmenu.DTOs;
using SMMS.Application.Features.foodmenu.Queries;

namespace SMMS.WebAPI.Controllers.Modules.KitchenStaff;

[ApiController]
[Route("api/kitchen")]
public class KitchenController : ControllerBase
{
    private readonly IMediator _mediator;

    public KitchenController(IMediator mediator)
    {
        _mediator = mediator;
    }

    private Guid GetSchoolIdFromToken()
    {
        var schoolIdClaim = User.FindFirst("SchoolId")?.Value;
        Console.WriteLine($"👉 [API DEBUG] Token Raw Claim 'SchoolId': {schoolIdClaim}");
        if (string.IsNullOrEmpty(schoolIdClaim))
            throw new UnauthorizedAccessException("Không tìm thấy SchoolId trong token.");

        var guid = Guid.Parse(schoolIdClaim);

        Console.WriteLine($"👉 [API DEBUG] Parsed SchoolId GUID: {guid}");

        return guid;
    }

    [HttpGet("dashboard")]
    public async Task<ActionResult<KitchenDashboardDto>> GetDashboard(
        [FromQuery] DateOnly? date)
    {
        var today = date ?? DateOnly.FromDateTime(DateTime.UtcNow);

        var query = new GetKitchenDashboardQuery(GetSchoolIdFromToken(), today);
        var result = await _mediator.Send(query);

        return Ok(result);
    }
}
