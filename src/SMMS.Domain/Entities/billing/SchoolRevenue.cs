using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using SMMS.Domain.Entities.auth;
using SMMS.Domain.Entities.school;

namespace SMMS.Domain.Entities.billing;

[Table("SchoolRevenues", Schema = "billing")]
[Index("SchoolId", "RevenueDate", Name = "IX_SchoolRevenues_SchoolDate")]
public partial class SchoolRevenue
{
    [Key]
    public long SchoolRevenueId { get; set; }

    public Guid SchoolId { get; set; }

    public DateOnly RevenueDate { get; set; }

    [Column(TypeName = "decimal(18, 2)")]
    public decimal RevenueAmount { get; set; }

    [StringLength(50)]
    public string? ContractCode { get; set; }

    [StringLength(500)]
    public string? ContractFileUrl { get; set; }

    [StringLength(1000)]
    public string? ContractNote { get; set; }

    public DateTime CreatedAt { get; set; }

    public Guid? CreatedBy { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public Guid? UpdatedBy { get; set; }

    public bool IsActive { get; set; }

    [ForeignKey("CreatedBy")]
    [InverseProperty("SchoolRevenueCreatedByNavigations")]
    public virtual User? CreatedByNavigation { get; set; }

    [ForeignKey("SchoolId")]
    [InverseProperty("SchoolRevenues")]
    public virtual School School { get; set; } = null!;

    [ForeignKey("UpdatedBy")]
    [InverseProperty("SchoolRevenueUpdatedByNavigations")]
    public virtual User? UpdatedByNavigation { get; set; }
}
