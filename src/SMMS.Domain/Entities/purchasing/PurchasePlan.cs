using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using SMMS.Domain.Entities.auth;
using SMMS.Domain.Entities.foodmenu;

namespace SMMS.Domain.Entities.purchasing;

[Table("PurchasePlans", Schema = "purchasing")]
public partial class PurchasePlan
{
    [Key]
    public int PlanId { get; set; }

    public DateTime GeneratedAt { get; set; }

    [StringLength(20)]
    public string PlanStatus { get; set; } = null!;

    public Guid StaffId { get; set; }

    public Guid? ConfirmedBy { get; set; }

    public DateTime? ConfirmedAt { get; set; }

    public bool AskToDelete { get; set; }

    public long? ScheduleMealId { get; set; }

    [ForeignKey("ConfirmedBy")]
    [InverseProperty("PurchasePlanConfirmedByNavigations")]
    public virtual User? ConfirmedByNavigation { get; set; }

    [InverseProperty("Plan")]
    public virtual ICollection<PurchaseOrder> PurchaseOrders { get; set; } = new List<PurchaseOrder>();

    [InverseProperty("Plan")]
    public virtual ICollection<PurchasePlanLine> PurchasePlanLines { get; set; } = new List<PurchasePlanLine>();

    [ForeignKey("ScheduleMealId")]
    [InverseProperty("PurchasePlans")]
    public virtual ScheduleMeal? ScheduleMeal { get; set; }

    [ForeignKey("StaffId")]
    [InverseProperty("PurchasePlanStaffs")]
    public virtual User Staff { get; set; } = null!;
}
