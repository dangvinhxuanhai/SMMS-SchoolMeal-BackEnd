using MediatR;
using Microsoft.AspNetCore.Mvc;
using SMMS.Application.Features.foodmenu.Commands;

namespace SMMS.WebAPI.Controllers.Modules.Manager;

[ApiController]
[Route("api/manager/ai-menu")]
public class AiMenuAdminController : ControllerBase
{
    private readonly IMediator _mediator;

    public AiMenuAdminController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Admin bấm nút gen file AI cho tất cả trường có NeedRebuildAiIndex = 0.
    /// </summary>
    [HttpPost("rebuild")]
    public async Task<IActionResult> RebuildAiFiles(CancellationToken ct)
    {
        await _mediator.Send(new GenerateAiFilesCommand(), ct);

        return Ok(new
        {
            message = "Đã gửi yêu cầu gen file AI cho các trường pending (NeedRebuildAiIndex = 0)."
        });
    }
}
