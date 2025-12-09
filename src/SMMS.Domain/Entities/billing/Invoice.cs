using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using SMMS.Domain.Entities.school;

namespace SMMS.Domain.Entities.billing;

[Table("Invoices", Schema = "billing")]
[Index("InvoiceCode", Name = "IX_Invoices_InvoiceCode", IsUnique = true)]
public partial class Invoice
{
    [Key]
    public long InvoiceId { get; set; }

    public Guid StudentId { get; set; }

    public short MonthNo { get; set; }

    public DateOnly DateFrom { get; set; }

    public DateOnly DateTo { get; set; }

    public int AbsentDay { get; set; }

    [StringLength(20)]
    public string Status { get; set; } = null!;

    public Guid InvoiceCode { get; set; }

    [InverseProperty("Invoice")]
    public virtual ICollection<Payment> Payments { get; set; } = new List<Payment>();

    [ForeignKey("StudentId")]
    [InverseProperty("Invoices")]
    public virtual Student Student { get; set; } = null!;
}
