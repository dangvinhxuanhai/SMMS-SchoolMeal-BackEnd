using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using SMMS.Domain.Entities.auth;
using SMMS.Domain.Entities.school;

namespace SMMS.Domain.Entities.purchasing;

[Table("PurchaseOrders", Schema = "purchasing")]
public partial class PurchaseOrder
{
    [Key]
    public int OrderId { get; set; }

    public Guid SchoolId { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime OrderDate { get; set; }

    [StringLength(50)]
    public string? PurchaseOrderStatus { get; set; }

    [StringLength(255)]
    public string? SupplierName { get; set; }

    public string? Note { get; set; }

    public int? PlanId { get; set; }

    public Guid? StaffInCharged { get; set; }

    [ForeignKey("PlanId")]
    [InverseProperty("PurchaseOrders")]
    public virtual PurchasePlan? Plan { get; set; }

    [InverseProperty("Order")]
    public virtual ICollection<PurchaseOrderLine> PurchaseOrderLines { get; set; } = new List<PurchaseOrderLine>();

    [ForeignKey("SchoolId")]
    [InverseProperty("PurchaseOrders")]
    public virtual School School { get; set; } = null!;

    [ForeignKey("StaffInCharged")]
    [InverseProperty("PurchaseOrders")]
    public virtual User? StaffInChargedNavigation { get; set; }
}
