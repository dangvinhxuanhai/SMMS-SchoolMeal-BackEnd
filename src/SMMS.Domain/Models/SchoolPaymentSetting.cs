using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace SMMS.Domain.Models;

[Table("SchoolPaymentSettings", Schema = "billing")]
public partial class SchoolPaymentSetting
{
    [Key]
    public int SettingId { get; set; }

    public Guid SchoolId { get; set; }

    public byte FromMonth { get; set; }

    public byte ToMonth { get; set; }

    [Column(TypeName = "decimal(18, 2)")]
    public decimal TotalAmount { get; set; }

    [StringLength(200)]
    public string? Note { get; set; }

    public bool IsActive { get; set; }

    public DateTime CreatedAt { get; set; }

    [ForeignKey("SchoolId")]
    [InverseProperty("SchoolPaymentSettings")]
    public virtual School School { get; set; } = null!;
}
