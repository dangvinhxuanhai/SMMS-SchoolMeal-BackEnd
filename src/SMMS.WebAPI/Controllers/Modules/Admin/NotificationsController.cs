using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.EntityFrameworkCore;
using SMMS.Application.Features.billing.DTOs;
using SMMS.Application.Features.notification.Interfaces;
using SMMS.Domain.Entities.auth;
using SMMS.Domain.Entities.billing;
using SMMS.Persistence.DbContextSite;

namespace SMMS.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Admin")]
    public class NotificationsController : ControllerBase
    {
        private readonly INotificationRepository _notificationRepo;
        private readonly EduMealContext _context;

        public NotificationsController(INotificationRepository notificationRepo, EduMealContext context)
        {
            _notificationRepo = notificationRepo;
            _context = context;
        }

        /// <summary>
        /// ✅ Tạo thông báo bảo trì (gửi cho toàn bộ người dùng)
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateNotificationDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim))
                return Unauthorized("Không tìm thấy thông tin admin đang đăng nhập.");

            var adminId = Guid.Parse(userIdClaim);

            // ✅ Kiểm tra giá trị SendType hợp lệ
            var validSendTypes = new[] { "Recurring", "Scheduled", "Immediate" };
            var sendType = dto.SendType;

            if (string.IsNullOrWhiteSpace(sendType) || !validSendTypes.Contains(sendType))
            {
                sendType = "Immediate"; // fallback mặc định
            }

            var notification = new Notification
            {
                Title = dto.Title,
                Content = dto.Content,
                AttachmentUrl = dto.AttachmentUrl,
                SenderId = adminId,
                SendType = sendType,
                CreatedAt = DateTime.UtcNow
            };

            var allUsers = await _context.Users.ToListAsync();
            foreach (var user in allUsers)
            {
                notification.NotificationRecipients.Add(new NotificationRecipient
                {
                    UserId = user.UserId,
                    IsRead = false
                });
            }

            await _notificationRepo.AddNotificationAsync(notification);

            return Ok(new
            {
                message = $"Notification ({sendType}) sent to {allUsers.Count} users."
            });
        }


        /// <summary>
        /// ✅ Xem lịch sử thông báo bảo trì (có tổng số người đã đọc)
        /// </summary>
        [EnableQuery]
        [HttpGet("history")]
        public IActionResult GetHistory()
        {
            var notifications = _notificationRepo.GetAllNotifications()
                .Select(n => new NotificationDto
                {
                    NotificationId = n.NotificationId,
                    Title = n.Title,
                    Content = n.Content,
                    AttachmentUrl = n.AttachmentUrl,
                    SendType = n.SendType,
                    CreatedAt = n.CreatedAt,
                    TotalRecipients = n.NotificationRecipients.Count(),
                    TotalRead = n.NotificationRecipients.Count(r => r.IsRead)
                })
                .OrderByDescending(n => n.CreatedAt);

            return Ok(notifications);
        }

        /// <summary>
        /// ✅ Xem chi tiết thông báo (ai đã đọc)
        /// </summary>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(long id)
        {
            var notification = await _notificationRepo.GetByIdAsync(id);
            if (notification == null)
                return NotFound();

            var detail = new
            {
                notification.NotificationId,
                notification.Title,
                notification.Content,
                notification.CreatedAt,
                Recipients = notification.NotificationRecipients.Select(r => new
                {
                    r.UserId,
                    UserEmail = r.User.Email,
                    r.IsRead,
                    r.ReadAt
                })
            };

            return Ok(detail);
        }
    }
}
