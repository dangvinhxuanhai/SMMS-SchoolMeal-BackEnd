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

    /// <summary>
    /// Nhờ AI đề xuất danh sách món ăn (món chính + món phụ).
    /// </summary>
    [HttpPost("recommend")]
    public async Task<ActionResult<AiMenuRecommendResponse>> Recommend(
        [FromBody] SuggestMenuRequest request,
        CancellationToken ct)
    {
        var command = new SuggestMenuCommand(
            request.UserId,
            request.SchoolId,
            request.MainIngredientIds ?? new(),
            request.SideIngredientIds ?? new(),
            request.AvoidAllergenIds ?? new(),
            request.MaxMainKcal,
            request.MaxSideKcal,
            request.TopKMain ?? 5,
            request.TopKSide ?? 5
        );

        var result = await _mediator.Send(command, ct);
        return Ok(result); // chứa session_id + danh sách món
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
            request.UserId,
            request.SessionId,
            request.SelectedItems
        );

        await _mediator.Send(command, ct);
        return NoContent();
    }
}
