using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace SMMS.Domain.Entities;

public partial class Ingredient
{
    [Key]
    public int IngredientId { get; set; }

    [StringLength(100)]
    public string IngredientName { get; set; } = null!;

    [StringLength(100)]
    public string? IngredientType { get; set; }

    [Column(TypeName = "decimal(7, 2)")]
    public decimal? EnergyKcal { get; set; }

    [Column(TypeName = "decimal(7, 2)")]
    public decimal? ProteinG { get; set; }

    [Column(TypeName = "decimal(7, 2)")]
    public decimal? FatG { get; set; }

    [Column(TypeName = "decimal(7, 2)")]
    public decimal? CarbG { get; set; }

    public bool IsActive { get; set; }

    [InverseProperty("Ingredient")]
    public virtual ICollection<AllergeticIngredient> AllergeticIngredients { get; set; } = new List<AllergeticIngredient>();

    [InverseProperty("Ingredient")]
    public virtual ICollection<FoodItemIngredient> FoodItemIngredients { get; set; } = new List<FoodItemIngredient>();

    [InverseProperty("Ingredient")]
    public virtual ICollection<InventoryItem> InventoryItems { get; set; } = new List<InventoryItem>();

    [InverseProperty("Ingredient")]
    public virtual ICollection<PurchaseOrderLine> PurchaseOrderLines { get; set; } = new List<PurchaseOrderLine>();

    [InverseProperty("Ingredient")]
    public virtual ICollection<PurchasePlanLine> PurchasePlanLines { get; set; } = new List<PurchasePlanLine>();
}
