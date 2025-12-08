using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace SMMS.Domain.Entities.billing;

[Table("Payments", Schema = "billing")]
[Index("PaymentCode", Name = "IX_Payments_PaymentCode", IsUnique = true)]
public partial class Payment
{
    [Key]
    public long PaymentId { get; set; }

    public long InvoiceId { get; set; }

    [Column(TypeName = "decimal(18, 2)")]
    public decimal ExpectedAmount { get; set; }

    [Column(TypeName = "decimal(18, 2)")]
    public decimal PaidAmount { get; set; }

    [StringLength(10)]
    public string PaymentStatus { get; set; } = null!;

    [StringLength(500)]
    public string? PaymentContent { get; set; }

    public DateTime PaidAt { get; set; }

    [StringLength(20)]
    public string Method { get; set; } = null!;

    [StringLength(100)]
    public string? ReferenceNo { get; set; }

    public Guid PaymentCode { get; set; }

    [ForeignKey("InvoiceId")]
    [InverseProperty("Payments")]
    public virtual Invoice Invoice { get; set; } = null!;
}
