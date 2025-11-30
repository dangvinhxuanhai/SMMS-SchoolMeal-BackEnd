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

    [HttpGet("dashboard")]
    public async Task<ActionResult<KitchenDashboardDto>> GetDashboard(
        [FromQuery] Guid schoolId,
        [FromQuery] DateOnly? date)
    {
        var today = date ?? DateOnly.FromDateTime(DateTime.UtcNow);

        var query = new GetKitchenDashboardQuery(schoolId, today);
        var result = await _mediator.Send(query);

        return Ok(result);
    }
}
