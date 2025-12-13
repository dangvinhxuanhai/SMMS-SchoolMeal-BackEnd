using MediatR;
using Microsoft.AspNetCore.Mvc;
using SMMS.Application.Features.foodmenu.Commands;
using SMMS.Application.Features.school.Queries;

namespace SMMS.WebAPI.Controllers.Modules.Admin;

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

    // api nay tra ve true co nghia la dc phep nhan nut rebuild,
    // neu false nghia la khong co truong nao can rebuild
    [HttpGet("need-rebuild")]
    public async Task<IActionResult> AnyNeedRebuild()
    {
        bool any = await _mediator.Send(new GetAnyNeedRebuildQuery());
        return Ok(any); // trả true/false
    }
}
