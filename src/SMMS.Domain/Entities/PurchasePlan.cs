using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace SMMS.Domain.Entities;

public partial class PurchasePlan
{
    [Key]
    public int PlanId { get; set; }

    public DateTime GeneratedAt { get; set; }

    [StringLength(20)]
    public string PlanStatus { get; set; } = null!;

    public int MenuId { get; set; }

    public int StaffId { get; set; }

    [ForeignKey("MenuId")]
    [InverseProperty("PurchasePlans")]
    public virtual Menu Menu { get; set; } = null!;

    [InverseProperty("Plan")]
    public virtual ICollection<PurchaseOrder> PurchaseOrders { get; set; } = new List<PurchaseOrder>();

    [InverseProperty("Plan")]
    public virtual ICollection<PurchasePlanLine> PurchasePlanLines { get; set; } = new List<PurchasePlanLine>();

    [ForeignKey("StaffId")]
    [InverseProperty("PurchasePlans")]
    public virtual User Staff { get; set; } = null!;
}
