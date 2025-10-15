using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using SMMS.Domain.Models.auth;
using SMMS.Domain.Models.foodmenu;

namespace SMMS.Domain.Models.purchasing;

[Table("PurchasePlans", Schema = "purchasing")]
public partial class PurchasePlan
{
    [Key]
    public int PlanId { get; set; }

    public DateTime GeneratedAt { get; set; }

    [StringLength(20)]
    public string PlanStatus { get; set; } = null!;

    public int MenuId { get; set; }

    public Guid StaffId { get; set; }

    public Guid? ConfirmedBy { get; set; }

    public DateTime? ConfirmedAt { get; set; }

    public bool AskToDelete { get; set; }

    [ForeignKey("ConfirmedBy")]
    [InverseProperty("PurchasePlanConfirmedByNavigations")]
    public virtual User? ConfirmedByNavigation { get; set; }

    [ForeignKey("MenuId")]
    [InverseProperty("PurchasePlans")]
    public virtual Menu Menu { get; set; } = null!;

    [InverseProperty("Plan")]
    public virtual ICollection<PurchaseOrder> PurchaseOrders { get; set; } = new List<PurchaseOrder>();

    [InverseProperty("Plan")]
    public virtual ICollection<PurchasePlanLine> PurchasePlanLines { get; set; } = new List<PurchasePlanLine>();

    [ForeignKey("StaffId")]
    [InverseProperty("PurchasePlanStaffs")]
    public virtual User Staff { get; set; } = null!;
}
