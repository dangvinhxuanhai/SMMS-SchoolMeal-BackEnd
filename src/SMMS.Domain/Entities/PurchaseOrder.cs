using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace SMMS.Domain.Entities;

public partial class PurchaseOrder
{
    [Key]
    public int OrderId { get; set; }

    public int SchoolId { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime OrderDate { get; set; }

    [Column("poStatus")]
    [StringLength(50)]
    public string? PoStatus { get; set; }

    [StringLength(255)]
    public string? SupplierName { get; set; }

    public string? Note { get; set; }

    public int? PlanId { get; set; }

    public int UserId { get; set; }

    [ForeignKey("PlanId")]
    [InverseProperty("PurchaseOrders")]
    public virtual PurchasePlan? Plan { get; set; }

    [InverseProperty("Order")]
    public virtual ICollection<PurchaseOrderLine> PurchaseOrderLines { get; set; } = new List<PurchaseOrderLine>();

    [ForeignKey("SchoolId")]
    [InverseProperty("PurchaseOrders")]
    public virtual School School { get; set; } = null!;

    [ForeignKey("UserId")]
    [InverseProperty("PurchaseOrders")]
    public virtual User User { get; set; } = null!;
}
