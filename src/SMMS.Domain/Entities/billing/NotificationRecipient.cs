using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using SMMS.Domain.Entities.auth;

namespace SMMS.Domain.Entities.billing;

[PrimaryKey("NotificationId", "UserId")]
[Table("NotificationRecipients", Schema = "billing")]
public partial class NotificationRecipient
{
    [Key]
    public long NotificationId { get; set; }

    [Key]
    public Guid UserId { get; set; }

    public bool IsRead { get; set; }

    public DateTime? ReadAt { get; set; }

    [ForeignKey("NotificationId")]
    [InverseProperty("NotificationRecipients")]
    public virtual Notification Notification { get; set; } = null!;

    [ForeignKey("UserId")]
    [InverseProperty("NotificationRecipients")]
    public virtual User User { get; set; } = null!;
}
