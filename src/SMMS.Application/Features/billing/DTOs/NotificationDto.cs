using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SMMS.Application.Features.billing.DTOs
{
    public class CreateNotificationDto
    {
        public string Title { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public string? AttachmentUrl { get; set; }
        public string? SendType { get; set; }
    }

    public class NotificationDto
    {
        public long NotificationId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public string? AttachmentUrl { get; set; }

        public string SendType { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public int TotalRecipients { get; set; }
        public int TotalRead { get; set; }
        public string? SenderName { get; set; }
    }

    public class NotificationDetailDto
    {
        public long NotificationId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? SenderName { get; set; }
        public string Content { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public List<RecipientDto> Recipients { get; set; } = new();

    }

    public class RecipientDto
    {
        public Guid UserId { get; set; }
        public string? UserEmail { get; set; }
        public bool IsRead { get; set; }
        public DateTime? ReadAt { get; set; }
    }
    public class AdminNotificationDto
    {
        public long NotificationId { get; set; }
        public Guid SenderId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public string? AttachmentUrl { get; set; }
        public string SendType { get; set; } = "Immediate";
        public DateTime CreatedAt { get; set; }
    }
    public class PagedResult<T>
    {
        public List<T> Items { get; set; }
        public long TotalCount { get; set; }

        public PagedResult(List<T> items, long totalCount)
        {
            Items = items;
            TotalCount = totalCount;
        }
    }
}
