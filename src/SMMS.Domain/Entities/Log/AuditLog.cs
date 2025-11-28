using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using SMMS.Domain.Entities.auth;

namespace SMMS.Domain.Entities.Logs;

[Table("AuditLogs", Schema = "logs")]
public partial class AuditLog
{
    [Key]
    public Guid LogId { get; set; }

    public Guid UserId { get; set; }

    [StringLength(100)]
    public string ActionDesc { get; set; } = null!;

    [StringLength(128)]
    public string? TableName { get; set; }

    [StringLength(64)]
    public string? RecordId { get; set; }

    [StringLength(100)]
    public string? AttributeNamne { get; set; }

    public string? OldData { get; set; }

    public string? NewData { get; set; }

    [StringLength(100)]
    public string? ActionType { get; set; }

    public DateTime CreatedAt { get; set; }

    [ForeignKey("UserId")]
    [InverseProperty("AuditLogs")]
    public virtual User User { get; set; } = null!;
}

