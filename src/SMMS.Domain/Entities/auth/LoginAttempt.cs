using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace SMMS.Domain.Entities.auth;

[Table("LoginAttempts", Schema = "auth")]
public partial class LoginAttempt
{
    [Key]
    public long AttemptId { get; set; }

    public Guid? UserId { get; set; }

    [StringLength(255)]
    public string? UserName { get; set; }

    public DateTime AttemptAt { get; set; }

    [StringLength(45)]
    public string? IpAddress { get; set; }

    public bool Succeeded { get; set; }
}
