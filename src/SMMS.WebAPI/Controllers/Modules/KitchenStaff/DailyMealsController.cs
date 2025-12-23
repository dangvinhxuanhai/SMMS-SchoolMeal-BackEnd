using System.Security.Claims;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SMMS.Application.Features.Meal.Command;
using SMMS.Application.Features.Meal.DTOs;
using SMMS.Application.Features.Meal.Queries;
using SMMS.Persistence.Service;

namespace SMMS.WebAPI.Controllers.Modules.KitchenStaff;
[ApiController]
[Route("api/meal/daily-meals")]
[Authorize(Roles = "KitchenStaff,Manager")]
public class DailyMealsController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly CloudinaryService _cloudinary;

    public DailyMealsController(
        IMediator mediator,
        CloudinaryService cloudinary)
    {
        _mediator = mediator;
        _cloudinary = cloudinary;
    }

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

    /// <summary>
    /// Lấy chi tiết 1 DailyMeal để hiển thị popup
    /// </summary>
    [HttpGet("{dailyMealId:long}/detail")]
    public async Task<ActionResult<DailyMealDetailPopupDto>> GetDetail(
        long dailyMealId,
        CancellationToken ct)
    {
        try
        {
            var result = await _mediator.Send(
                new GetDailyMealDetailQuery(dailyMealId), ct);

            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(new { error = ex.Message });
        }
    }

    /// <summary>
    /// upload ảnh mẫu thử
    /// </summary>
    [HttpPost("{dailyMealId:int}/evidences")]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> Upload(
        int dailyMealId,
        [FromForm] CreateDailyMealEvidenceRequest request)
    {
        var url = await _cloudinary.UploadEnvImageAsync(request.File);

        var evidenceId = await _mediator.Send(
            new CreateDailyMealEvidenceCommand(
                dailyMealId,
                url,
                request.Caption,
                GetCurrentUserId()));

        return Ok(new
        {
            evidenceId,
            evidenceUrl = url
        });
    }

    /// <summary>
    /// xóa ảnh mẫu thử
    /// </summary>
    [HttpDelete("evidences/{evidenceId:long}")]
    public async Task<IActionResult> Delete(long evidenceId)
    {
        await _mediator.Send(
            new DeleteDailyMealEvidenceCommand(evidenceId));

        return NoContent();
    }

    /// <summary>
    /// update các actual ingredients theo dailymeal Id
    /// </summary>
    [HttpPut("{dailyMealId:int}/actual-ingredients")]
    public async Task<IActionResult> UpdateActualIngredients(
    int dailyMealId,
    [FromBody] UpdateDailyMealActualIngredientsRequest request)
    {
        await _mediator.Send(
            new UpdateDailyMealActualIngredientsCommand(
                dailyMealId,
                request.Items));

        return NoContent();
    }
}
