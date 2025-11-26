using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SMMS.Application.Features.Manager.DTOs;
public class ManagerNotificationDto
{
    public long NotificationId { get; set; }
    public Guid? SenderId { get; set; }

    public string Title { get; set; } = default!;
    public string Content { get; set; } = default!;
    public string? AttachmentUrl { get; set; }

    public string SendType { get; set; } = "Immediate"; // Immediate / Scheduled / Recurring
    public string? ScheduleCron { get; set; }

    public DateTime CreatedAt { get; set; }

    public int TotalRecipients { get; set; }
}

public class CreateManagerNotificationRequest
{

    public string Title { get; set; } = default!;
    public string Content { get; set; } = default!;
    public string? AttachmentUrl { get; set; }

    // Target groups
    public bool SendToParents { get; set; } = true;
    public bool SendToKitchenStaff { get; set; } = false;
    public bool SendToTeachers { get; set; } = false;

    // Optional: scheduled
    public string SendType { get; set; } = "Immediate"; // "Immediate" | "Scheduled" | "Recurring"
    public string? ScheduleCron { get; set; }
}

public class UpdateManagerNotificationRequest
{
    public string Title { get; set; } = default!;
    public string Content { get; set; } = default!;
    public string? AttachmentUrl { get; set; }

    // Cho phép đổi loại gửi nếu muốn
    public string SendType { get; set; } = "Immediate";
    public string? ScheduleCron { get; set; }
}
