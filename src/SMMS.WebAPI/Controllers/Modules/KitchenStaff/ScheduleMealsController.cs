using System.Security.Claims;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SMMS.Application.Features.foodmenu.DTOs;
using SMMS.Application.Features.foodmenu.Queries;
using SMMS.Application.Features.Meal.Command;
using SMMS.Application.Features.Meal.DTOs;
using SMMS.Application.Features.Meal.Queries;

namespace SMMS.WebAPI.Controllers.Modules.KitchenStaff;

[ApiController]
[Route("api/meal/[controller]")]
[Authorize(Roles = "KitchenStaff")]
public class ScheduleMealsController : ControllerBase
{
    private readonly IMediator _mediator;

    public ScheduleMealsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Lấy danh sách các tuần (ScheduleMeal) của school hiện tại, có phân trang.
    /// Mỗi item gồm ScheduleMeal + list DailyMeals.
    /// </summary>
    /// GET /api/meal/ScheduleMeals?pageIndex=1&pageSize=4
    [HttpGet]
    public async Task<ActionResult<PagedResult<WeeklyScheduleDto>>> GetPaged(
        [FromQuery] int pageIndex = 1,
        [FromQuery] int pageSize = 4,
        [FromQuery] bool GetAll = false,
        CancellationToken ct = default)
    {
        var schoolId = GetSchoolIdFromToken();

        var query = new GetWeeklySchedulesPagedQuery(
            SchoolId: schoolId,
            PageIndex: pageIndex,
            PageSize: pageSize,
            GetAll: GetAll
        );

        var result = await _mediator.Send(query, ct);
        if (result == null)
            return NotFound();
        return Ok(result);
    }

    /// <summary>
    /// Lấy tuần chứa một ngày bất kỳ (thường là hôm nay) cho school hiện tại.
    /// </summary>
    /// GET /api/meal/ScheduleMeals/week-of?date=2025-11-30
    [HttpGet("week-of")]
    public async Task<ActionResult<WeeklyScheduleDto>> GetWeekOf(
        [FromQuery] DateTime date,
        CancellationToken ct = default)
    {
        var schoolId = GetSchoolIdFromToken();

        var query = new GetWeeklyScheduleByDateQuery(
            SchoolId: schoolId,
            Date: date
        );

        var result = await _mediator.Send(query, ct);
        if (result == null)
            return NotFound();

        return Ok(result);
    }

    [HttpPost]
    public async Task<ActionResult<long>> Create(
        [FromBody] CreateScheduleMealCommand command,
        CancellationToken ct)
    {

        try
        {
            // Lấy SchoolId & UserId từ token, không tin từ client
            command.SchoolId = GetSchoolIdFromToken();
            command.CreatedByUserId = GetCurrentUserId();
            var id = await _mediator.Send(command, ct);
            return Ok(new { scheduleMealId = id });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Check ngày nghỉ trong khoảng thời gian
    /// </summary>
    /// GET /api/meal/ScheduleMeals/check?fromDate=2025-03-10&toDate=2025-03-16
    [HttpGet("check")]
    public async Task<ActionResult<OffDateCheckResultDto>> Check(
        [FromQuery] DateOnly fromDate,
        [FromQuery] DateOnly toDate,
        CancellationToken ct)
    {
        try
        {
            var result = await _mediator.Send(
                new CheckOffDatesQuery(
                    fromDate,
                    toDate,
                    GetSchoolIdFromToken()),
                ct);

            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    // ================== helpers lấy claim ==================

    private Guid GetSchoolIdFromToken()
    {
        var schoolIdClaim = User.FindFirst("SchoolId")?.Value;
        if (string.IsNullOrEmpty(schoolIdClaim))
            throw new UnauthorizedAccessException("Không tìm thấy SchoolId trong token.");

        return Guid.Parse(schoolIdClaim);
    }

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
