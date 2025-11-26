using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace SMMS.Domain.Entities.auth;

[Table("RefreshTokens", Schema = "auth")]
[Index("Token", Name = "UQ__RefreshT__1EB4F817E3CAB654", IsUnique = true)]
public partial class RefreshToken
{
    [Key]
    public long RefreshTokenId { get; set; }

    public Guid UserId { get; set; }

    [StringLength(250)]
    public string Token { get; set; } = null!;

    public DateTime ExpiresAt { get; set; }

    public DateTime CreatedAt { get; set; }

    [StringLength(45)]
    public string? CreatedByIp { get; set; }

    public DateTime? RevokedAt { get; set; }

    [StringLength(45)]
    public string? RevokedByIp { get; set; }

    public long? ReplacedById { get; set; }

    [InverseProperty("ReplacedBy")]
    public virtual ICollection<RefreshToken> InverseReplacedBy { get; set; } = new List<RefreshToken>();

    [ForeignKey("ReplacedById")]
    [InverseProperty("InverseReplacedBy")]
    public virtual RefreshToken? ReplacedBy { get; set; }

    [ForeignKey("UserId")]
    [InverseProperty("RefreshTokens")]
    public virtual User User { get; set; } = null!;
}
