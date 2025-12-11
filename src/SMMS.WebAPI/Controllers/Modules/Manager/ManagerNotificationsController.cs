using System.Security.Claims;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SMMS.Application.Features.auth.Interfaces;
using SMMS.Application.Features.Manager.Commands;
using SMMS.Application.Features.Manager.DTOs;
using SMMS.Application.Features.Manager.Queries;
using SMMS.Application.Features.Manager.Interfaces;
using SMMS.Application.Features.Wardens.Queries;

namespace SMMS.WebAPI.Controllers.Modules.Manager;

[Route("api/[controller]")]
[ApiController]
[Authorize(Roles = "Manager")]
public class ManagerNotificationsController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly IUserRepository _userRepository;
    private readonly INotificationRealtimeService _realtimeService;

    public ManagerNotificationsController(IMediator mediator, IUserRepository userRepository,
        INotificationRealtimeService realtimeService)
    {
        _mediator = mediator;
        _realtimeService = realtimeService;
        _userRepository = userRepository;
    }

    #region Helpers

    private Guid GetSchoolIdFromToken()
    {
        var schoolIdClaim = User.FindFirst("SchoolId")?.Value;
        if (string.IsNullOrEmpty(schoolIdClaim))
            throw new UnauthorizedAccessException("Không tìm thấy SchoolId trong token.");

        return Guid.Parse(schoolIdClaim);
    }

    private Guid GetCurrentUserId()
    {
        // Trong JwtTokenService bạn đang dùng ClaimTypes.NameIdentifier cho UserId
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (string.IsNullOrEmpty(userIdClaim))
            throw new UnauthorizedAccessException("Không tìm thấy UserId trong token.");

        return Guid.Parse(userIdClaim);
    }

    #endregion

    // 1️⃣ CREATE: Tạo thông báo mới
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateManagerNotificationRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var schoolId = GetSchoolIdFromToken();
        var senderId = GetCurrentUserId();

        var result = await _mediator.Send(
            new CreateManagerNotificationCommand(request, senderId, schoolId));

        return Ok(new { message = "Tạo thông báo thành công!", data = result });
    }

    // 2️⃣ UPDATE: Cập nhật thông báo
    [HttpPut("{notificationId:long}")]
    public async Task<IActionResult> Update(
        long notificationId,
        [FromBody] UpdateManagerNotificationRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var senderId = GetCurrentUserId(); // lấy từ token
        var schoolId = GetSchoolIdFromToken(); // lấy từ token

        var result = await _mediator.Send(
            new UpdateManagerNotificationCommand(
                notificationId,
                request,
                senderId,
                schoolId
            ));

        if (result == null)
            return NotFound(new { message = "Không tìm thấy thông báo cần cập nhật." });

        return Ok(new { message = "Cập nhật thông báo thành công!", data = result });
    }

    // 3️⃣ DELETE: Xoá thông báo
    [HttpDelete("{notificationId:long}")]
    public async Task<IActionResult> Delete(long notificationId)
    {
        var senderId = GetCurrentUserId();

        var success = await _mediator.Send(
            new DeleteManagerNotificationCommand(notificationId, senderId));

        if (!success)
            return NotFound(new { message = "Không tìm thấy thông báo để xoá hoặc bạn không có quyền." });

        return Ok(new { message = "Xoá thông báo thành công!" });
    }

    // 4️⃣ GET BY ID
    [HttpGet("{notificationId:long}")]
    public async Task<IActionResult> GetById(long notificationId)
    {
        // Nếu record query của bạn là: record GetManagerNotificationByIdQuery(long NotificationId)
        var result = await _mediator.Send(
            new GetManagerNotificationByIdQuery(notificationId));

        if (result == null)
            return NotFound(new { message = "Không tìm thấy thông báo." });

        return Ok(new { data = result });
    }

    // 5️⃣ GET LIST BY CURRENT MANAGER (PAGING)
    [HttpGet]
    public async Task<IActionResult> GetMyNotifications(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        if (page <= 0) page = 1;
        if (pageSize <= 0) pageSize = 20;

        var senderId = GetCurrentUserId();

        var result = await _mediator.Send(
            new GetManagerNotificationsBySenderQuery(senderId, page, pageSize));

        return Ok(new {
            page,
            pageSize,
            count = result.TotalCount,
            data = result.Items
        });
    }

    [HttpGet("notifications")]
    public async Task<IActionResult> GetNotifications()
    {
        try
        {
            var managerId = GetCurrentUserId();
            var notifications = await _mediator.Send(
                new GetWardenNotificationsQuery(managerId));

            return Ok(notifications);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
}
