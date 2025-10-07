using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace SMMS.Domain.Entities;

public partial class InventoryItem
{
    [Key]
    public int ItemId { get; set; }

    public int IngredientId { get; set; }

    public int SchoolId { get; set; }

    [StringLength(150)]
    public string? ItemName { get; set; }

    [Column(TypeName = "decimal(12, 2)")]
    public decimal QuantityGram { get; set; }

    public DateOnly? ExpirationDate { get; set; }

    [StringLength(100)]
    public string? BatchNo { get; set; }

    [StringLength(255)]
    public string? Origin { get; set; }

    public DateTime LastUpdated { get; set; }

    [ForeignKey("IngredientId")]
    [InverseProperty("InventoryItems")]
    public virtual Ingredient Ingredient { get; set; } = null!;

    [InverseProperty("Item")]
    public virtual ICollection<InventoryTransaction> InventoryTransactions { get; set; } = new List<InventoryTransaction>();

    [ForeignKey("SchoolId")]
    [InverseProperty("InventoryItems")]
    public virtual School School { get; set; } = null!;
}
