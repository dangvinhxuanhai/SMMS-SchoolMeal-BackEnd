using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace SMMS.Domain.Entities.inventory;

[Table("InventoryTransactions", Schema = "inventory")]
public partial class InventoryTransaction
{
    [Key]
    public long TransId { get; set; }

    public int ItemId { get; set; }

    [StringLength(10)]
    public string TransType { get; set; } = null!;

    [Column(TypeName = "decimal(12, 2)")]
    public decimal QuantityGram { get; set; }

    public DateTime TransDate { get; set; }

    [StringLength(100)]
    public string? Reference { get; set; }

    [ForeignKey("ItemId")]
    [InverseProperty("InventoryTransactions")]
    public virtual InventoryItem Item { get; set; } = null!;
}
