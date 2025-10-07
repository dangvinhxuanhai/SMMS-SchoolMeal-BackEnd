using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace SMMS.Domain.Entities;

public partial class AuditLog
{
    [Key]
    public long LogId { get; set; }

    public int UserId { get; set; }

    [StringLength(100)]
    public string ActionDesc { get; set; } = null!;

    [StringLength(128)]
    public string? TableName { get; set; }

    [StringLength(64)]
    public string? RecordId { get; set; }

    public string? OldData { get; set; }

    public string? NewData { get; set; }

    [StringLength(100)]
    public string? ActionType { get; set; }

    public DateTime CreatedAt { get; set; }

    [ForeignKey("UserId")]
    [InverseProperty("AuditLogs")]
    public virtual User User { get; set; } = null!;
}
