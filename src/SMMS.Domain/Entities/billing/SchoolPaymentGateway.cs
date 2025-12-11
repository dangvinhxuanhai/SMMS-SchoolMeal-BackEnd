using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using SMMS.Domain.Entities.auth;
using SMMS.Domain.Entities.school;

namespace SMMS.Domain.Entities.billing;

[Table("SchoolPaymentGateways", Schema = "billing")]
public partial class SchoolPaymentGateway
{
    [Key]
    public long GatewayId { get; set; }

    public Guid SchoolId { get; set; }

    [StringLength(30)]
    public string TheProvider { get; set; } = null!;

    [StringLength(200)]
    public string? ClientId { get; set; }

    [StringLength(200)]
    public string? ApiKey { get; set; }

    [StringLength(200)]
    public string? ChecksumKey { get; set; }

    [StringLength(300)]
    public string? ReturnUrl { get; set; }

    [StringLength(300)]
    public string? CancelUrl { get; set; }

    public bool IsActive { get; set; }

    public DateTime CreatedAt { get; set; }

    public Guid? CreatedBy { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public Guid? UpdatedBy { get; set; }

    [ForeignKey("CreatedBy")]
    [InverseProperty("SchoolPaymentGatewayCreatedByNavigations")]
    public virtual User? CreatedByNavigation { get; set; }

    [ForeignKey("SchoolId")]
    [InverseProperty("SchoolPaymentGateways")]
    public virtual School School { get; set; } = null!;

    [ForeignKey("UpdatedBy")]
    [InverseProperty("SchoolPaymentGatewayUpdatedByNavigations")]
    public virtual User? UpdatedByNavigation { get; set; }
}
