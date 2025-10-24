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
    }
}
