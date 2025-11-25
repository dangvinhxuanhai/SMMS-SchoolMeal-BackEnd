using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace SMMS.Domain.Entities.auth;

[Table("ExternalProviders", Schema = "auth")]
[Index("ProviderName", Name = "UQ__External__7D057CE505536EA9", IsUnique = true)]
public partial class ExternalProvider
{
    [Key]
    public short ProviderId { get; set; }

    [StringLength(50)]
    public string ProviderName { get; set; } = null!;

    public string? KeyId { get; set; }

    [InverseProperty("Provider")]
    public virtual ICollection<UserExternalLogin> UserExternalLogins { get; set; } = new List<UserExternalLogin>();
}
