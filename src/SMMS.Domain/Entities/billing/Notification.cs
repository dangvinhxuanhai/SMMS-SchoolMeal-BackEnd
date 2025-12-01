using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using SMMS.Domain.Entities.auth;

namespace SMMS.Domain.Entities.billing;

[Table("notifications", Schema = "billing")]
public partial class Notification
{
    [Key]
    public long NotificationId { get; set; }

    [StringLength(150)]
    public string Title { get; set; } = null!;

    public string Content { get; set; } = null!;

    [StringLength(300)]
    public string? AttachmentUrl { get; set; }

    public Guid SenderId { get; set; }

    [StringLength(20)]
    public string SendType { get; set; } = null!;

    [StringLength(100)]
    public string? ScheduleCron { get; set; }

    public DateTime CreatedAt { get; set; }


    [InverseProperty("Notification")]
    public virtual ICollection<NotificationRecipient> NotificationRecipients { get; set; } = new List<NotificationRecipient>();

    [ForeignKey("SenderId")]
    [InverseProperty("Notifications")]
    public virtual User? Sender { get; set; }
}
