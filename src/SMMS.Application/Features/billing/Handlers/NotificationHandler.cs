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
        IRequestHandler<CreateNotificationCommand, AdminNotificationDto>,
        IRequestHandler<GetNotificationHistoryQuery, IEnumerable<NotificationDto>>,
        IRequestHandler<GetNotificationByIdQuery, NotificationDetailDto?>,
        IRequestHandler<DeleteNotificationCommand, bool>
    {
        private readonly INotificationRepository _notificationRepo;

        public NotificationHandler(INotificationRepository notificationRepo)
        {
            _notificationRepo = notificationRepo;
        }

        public async Task<AdminNotificationDto> Handle(CreateNotificationCommand request,
            CancellationToken cancellationToken)
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

            return new AdminNotificationDto
            {
                NotificationId = notification.NotificationId,
                Title = notification.Title,
                Content = notification.Content,
                AttachmentUrl = notification.AttachmentUrl,
                SenderId = notification.SenderId,
                CreatedAt = notification.CreatedAt
            };
        }

        public async Task<IEnumerable<NotificationDto>> Handle(GetNotificationHistoryQuery request,
            CancellationToken cancellationToken)
        {
            var data = _notificationRepo.GetAllNotifications()
                .Include(n => n.Sender)
                .Where(n => n.SenderId == request.adminId); // chỉ lấy thông báo của Admin đang login

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
                    TotalRead = n.NotificationRecipients.Count(r => r.IsRead),
                    SenderName = n.Sender != null ? n.Sender.FullName : null
                })
                .OrderByDescending(n => n.CreatedAt)
                .ToListAsync(cancellationToken);
        }


        public async Task<NotificationDetailDto?> Handle(GetNotificationByIdQuery request,
            CancellationToken cancellationToken)
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
                    UserId = r.UserId, UserEmail = r.User.Email, IsRead = r.IsRead, ReadAt = r.ReadAt
                }).ToList()
            };
        }

        public async Task<bool> Handle(DeleteNotificationCommand request, CancellationToken cancellationToken)
        {
            var notification = await _notificationRepo.GetByIdAsync(request.NotificationId);

            if (notification == null)
                return false;

            // CHỈ admin tạo ra thông báo mới được xóa
            if (notification.SenderId != request.AdminId)
                throw new UnauthorizedAccessException("Bạn không có quyền xóa thông báo này.");

            await _notificationRepo.DeleteNotificationAsync(notification);
            return true;
        }
    }
}
