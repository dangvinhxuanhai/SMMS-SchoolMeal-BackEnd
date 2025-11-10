using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediatR;
using SMMS.Application.Features.billing.Commands;
using SMMS.Application.Features.billing.DTOs;
using SMMS.Application.Features.billing.Queries;
using SMMS.Application.Features.notification.Interfaces;
using SMMS.Domain.Entities.billing;
using Microsoft.EntityFrameworkCore;
namespace SMMS.Application.Features.billing.Handlers
{
    public class NotificationHandler :
      IRequestHandler<CreateNotificationCommand, string>,
      IRequestHandler<GetNotificationHistoryQuery, IEnumerable<NotificationDto>>,
      IRequestHandler<GetNotificationByIdQuery, NotificationDetailDto?>
    {
        private readonly INotificationRepository _notificationRepo;

        public NotificationHandler(INotificationRepository notificationRepo)
        {
            _notificationRepo = notificationRepo;
        }

        public async Task<string> Handle(CreateNotificationCommand request, CancellationToken cancellationToken)
        {
            var dto = request.Dto;
            var adminId = request.AdminId;

            var validSendTypes = new[] { "Recurring", "Scheduled", "Immediate" };
            var sendType = validSendTypes.Contains(dto.SendType) ? dto.SendType : "Immediate";

            var notification = new Notification
            {
                Title = dto.Title,
                Content = dto.Content,
                AttachmentUrl = dto.AttachmentUrl,
                SenderId = adminId,
                SendType = sendType,
                CreatedAt = DateTime.UtcNow
            };

            await _notificationRepo.AddNotificationAsync(notification);

            return $"Notification ({sendType}) created successfully.";
        }

        public async Task<IEnumerable<NotificationDto>> Handle(GetNotificationHistoryQuery request, CancellationToken cancellationToken)
        {
            var data = _notificationRepo.GetAllNotifications();

            return await data
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
                .OrderByDescending(n => n.CreatedAt)
                .ToListAsync(cancellationToken);
        }

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
                Recipients = notification.NotificationRecipients.Select(r => new RecipientDto
                {
                    UserId = r.UserId,
                    UserEmail = r.User.Email,
                    IsRead = r.IsRead,
                    ReadAt = r.ReadAt
                }).ToList()
            };
        }
    }
}
