using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace SMMS.Domain.Entities.auth;

[PrimaryKey("UserId", "ProviderId", "ProviderSub")]
[Table("UserExternalLogins", Schema = "auth")]
public partial class UserExternalLogin
{
    [Key]
    public Guid UserId { get; set; }

    [Key]
    public short ProviderId { get; set; }

    [Key]
    [StringLength(100)]
    public string ProviderSub { get; set; } = null!;

    [StringLength(255)]
    public string? Email { get; set; }

    public DateTime CreatedAt { get; set; }

    [ForeignKey("ProviderId")]
    [InverseProperty("UserExternalLogins")]
    public virtual ExternalProvider Provider { get; set; } = null!;

    [ForeignKey("UserId")]
    [InverseProperty("UserExternalLogins")]
    public virtual User User { get; set; } = null!;
}
