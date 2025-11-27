using MediatR;
using SMMS.Application.Features.billing.Commands;
using SMMS.Application.Features.billing.DTOs;
using SMMS.Application.Features.billing.Queries;
using SMMS.Domain.Entities.billing;
using Microsoft.EntityFrameworkCore;
using SMMS.Application.Features.Manager.Interfaces;
using SMMS.Application.Features.billing.Interfaces;
namespace SMMS.Application.Features.billing.Handlers
{
    public class NotificationHandler :
       IRequestHandler<CreateNotificationCommand, AdminNotificationDto>,
       IRequestHandler<GetNotificationHistoryQuery, IEnumerable<NotificationDto>>,
       IRequestHandler<GetNotificationByIdQuery, NotificationDetailDto?>
    {
        private readonly INotificationRepository _notificationRepo;
        private readonly INotificationARealtimeService _realtime;

        public NotificationHandler(
            INotificationRepository notificationRepo,
            INotificationARealtimeService realtime)
        {
            _notificationRepo = notificationRepo;
            _realtime = realtime;
        }

        // 1️⃣ CREATE
        public async Task<AdminNotificationDto> Handle(CreateNotificationCommand request, CancellationToken cancellationToken)
        {
            var dto = request.Dto;
            var adminId = request.AdminId;

            var notification = new Notification
            {
                Title = dto.Title,
                Content = dto.Content,
                AttachmentUrl = dto.AttachmentUrl,
                SenderId = adminId,
                SendType = dto.SendType ?? "Immediate",
                CreatedAt = DateTime.UtcNow
            };

            // Thêm recipients cho tất cả user
            var users = await _notificationRepo.GetAllRecipientsUserIdsAsync();
            foreach (var userId in users)
            {
                notification.NotificationRecipients.Add(new NotificationRecipient
                {
                    UserId = userId,
                    IsRead = false
                });
            }

            await _notificationRepo.AddNotificationAsync(notification);

            // Map DTO trả về
            var notificationDto = new AdminNotificationDto
            {
                NotificationId = notification.NotificationId,
                SenderId = notification.SenderId.Value,
                Title = notification.Title,
                Content = notification.Content,
                AttachmentUrl = notification.AttachmentUrl,
                SendType = notification.SendType,
                CreatedAt = notification.CreatedAt
            };

            // Gửi realtime
            await _realtime.SendToUsersAsync(users, notificationDto);

            return notificationDto;
        }

        // 2️⃣ Get history
        public async Task<IEnumerable<NotificationDto>> Handle(GetNotificationHistoryQuery request, CancellationToken cancellationToken)
        {
            var data = _notificationRepo.GetAllNotifications();

            return await data
                .Select(n => new NotificationDto
                {
                    NotificationId = n.NotificationId,
                    Title = n.Title,
                    Content = n.Content,
                    SendType = n.SendType,
                    CreatedAt = n.CreatedAt,
                    TotalRecipients = n.NotificationRecipients.Count(),
                    TotalRead = n.NotificationRecipients.Count(r => r.IsRead)
                })
                .OrderByDescending(n => n.CreatedAt)
                .ToListAsync(cancellationToken);
        }

        // 3️⃣ Get by Id
        public async Task<NotificationDetailDto?> Handle(GetNotificationByIdQuery request, CancellationToken cancellationToken)
        {
            var notification = await _notificationRepo.GetByIdAsync(request.Id);
            if (notification == null) return null;

            return new NotificationDetailDto
            {
                NotificationId = notification.NotificationId,
                SenderName = notification.Sender?.FullName ?? "Không tìm thấy",
                Title = notification.Title,
                Content = notification.Content,
                CreatedAt = notification.CreatedAt,
                Recipients = notification.NotificationRecipients
                    .Where(r => r.UserId != Guid.Empty)
                    .Select(r => new RecipientDto
                    {
                        UserId = r.UserId,
                        UserEmail = r.User?.Email,
                        IsRead = r.IsRead,
                        ReadAt = r.ReadAt
                    }).ToList()
            };
        }
    }
}
