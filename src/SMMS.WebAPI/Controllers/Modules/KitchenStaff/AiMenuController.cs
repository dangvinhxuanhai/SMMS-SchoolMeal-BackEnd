using System.Security.Claims;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using SMMS.Application.Features.foodmenu.Commands;
using SMMS.Application.Features.foodmenu.DTOs;
using SMMS.WebAPI.DTOs;

namespace SMMS.WebAPI.Controllers.Modules.KitchenStaff;

[ApiController]
[Route("api/[controller]")]
public class AiMenuController : ControllerBase
{
    private readonly IMediator _mediator;

    public AiMenuController(IMediator mediator)
    {
        _mediator = mediator;
    }

    private Guid GetSchoolIdFromToken()
    {
        var schoolIdClaim = User.FindFirst("SchoolId")?.Value;
        if (string.IsNullOrEmpty(schoolIdClaim))
            throw new UnauthorizedAccessException("Không tìm thấy SchoolId trong token.");

        return Guid.Parse(schoolIdClaim);
        // return Guid.Parse("477B88C0-2E3E-48A8-AF4F-BD00C5DE9AA0");
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
    /// Nhờ AI đề xuất danh sách món ăn (món chính + món phụ).
    /// </summary>
    [HttpPost("recommend")]
    public async Task<ActionResult<AiMenuRecommendResponse>> Recommend(
        [FromBody] SuggestMenuRequest request,
        CancellationToken ct)
    {
        var realSchoolId = GetSchoolIdFromToken();
        var realUserId = GetCurrentUserId();

        Console.WriteLine($">>> SchoolID: {realSchoolId} | UserID: {realUserId}");

        var command = new SuggestMenuCommand(
            SchoolId: realSchoolId,
            UserId: realUserId,

            MainIngredientIds: request.MainIngredientIds ?? new(),
            SideIngredientIds: request.SideIngredientIds ?? new(),
            AvoidAllergenIds: request.AvoidAllergenIds ?? new(),
            MaxMainKcal: request.MaxMainKcal,
            MaxSideKcal: request.MaxSideKcal,
            TopKMain: request.TopKMain ?? 5,
            TopKSide: request.TopKSide ?? 5
        );

        var result = await _mediator.Send(command, ct);
        return Ok(result);
    }
    /// <summary>
    /// Ghi log user đã chọn món nào trong list recommend (phục vụ Machine Learning).
    /// </summary>
    [HttpPost("selection")]
    public async Task<IActionResult> LogSelection(
        [FromBody] LogAiSelectionRequest request,
        CancellationToken ct)
    {
        var command = new LogAiSelectionCommand(
            GetCurrentUserId(),
            request.SessionId,
            request.SelectedItems
        );

        await _mediator.Send(command, ct);
        return NoContent();
    }
}
