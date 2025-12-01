using System.Security.Claims;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SMMS.Application.Features.Meal.DTOs;
using SMMS.Application.Features.Meal.Queries;

namespace SMMS.WebAPI.Controllers.Modules.KitchenStaff;
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class MenusController : ControllerBase
{
    private readonly IMediator _mediator;

    public MenusController(IMediator mediator)
    {
        _mediator = mediator;
    }

    // GET api/menus?yearId=&weekNo=
    [HttpGet]
    public async Task<ActionResult<List<KsMenuListItemDto>>> GetAll(
        [FromQuery] int? yearId,
        [FromQuery] short? weekNo,
        CancellationToken ct)
    {
        var schoolId = GetSchoolIdFromToken();

        var query = new GetMenuListQuery
        {
            SchoolId = schoolId,
            YearId = yearId,
            WeekNo = weekNo
        };

        var result = await _mediator.Send(query, ct);
        return Ok(result);
    }

    // GET api/menus/{id}
    [HttpGet("{id:int}")]
    public async Task<ActionResult<KsMenuDetailDto>> GetDetail(
        int id,
        CancellationToken ct)
    {
        var schoolId = GetSchoolIdFromToken();

        var query = new GetMenuDetailQuery
        {
            MenuId = id,
            SchoolId = schoolId
        };

        var result = await _mediator.Send(query, ct);
        return Ok(result);
    }

    // ================= helper lấy claim =================

    private Guid GetSchoolIdFromToken()
    {
        var schoolIdClaim = User.FindFirst("SchoolId")?.Value;
        if (string.IsNullOrEmpty(schoolIdClaim))
            throw new UnauthorizedAccessException("Không tìm thấy SchoolId trong token.");

        return Guid.Parse(schoolIdClaim);
    }

    // Nếu sau này cần UserId thì dùng hàm này
    private Guid GetCurrentUserId()
    {
        var userIdString = User.FindFirst("UserId")?.Value
                           ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                           ?? User.FindFirst("sub")?.Value
                           ?? User.FindFirst("id")?.Value
                           ?? throw new Exception("Token does not contain UserId.");

        return Guid.Parse(userIdString);
    }
}
