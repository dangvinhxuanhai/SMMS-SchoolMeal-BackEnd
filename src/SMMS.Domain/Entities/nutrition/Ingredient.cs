using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using SMMS.Domain.Entities.auth;
using SMMS.Domain.Entities.fridge;
using SMMS.Domain.Entities.inventory;
using SMMS.Domain.Entities.purchasing;
using SMMS.Domain.Entities.school;

namespace SMMS.Domain.Entities.nutrition;

[Table("Ingredients", Schema = "nutrition")]
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

    public Guid SchoolId { get; set; }

    public Guid? CreatedBy { get; set; }

    public DateTime CreatedAt { get; set; }

    public bool IsActive { get; set; }

    [InverseProperty("Ingredient")]
    public virtual ICollection<AllergeticIngredient> AllergeticIngredients { get; set; } = new List<AllergeticIngredient>();

    [ForeignKey("CreatedBy")]
    [InverseProperty("Ingredients")]
    public virtual User? CreatedByNavigation { get; set; }

    [InverseProperty("Ingredient")]
    public virtual ICollection<FoodItemIngredient> FoodItemIngredients { get; set; } = new List<FoodItemIngredient>();

    [InverseProperty("AltIngredient")]
    public virtual ICollection<IngredientAlternative> IngredientAlternativeAltIngredients { get; set; } = new List<IngredientAlternative>();

    [InverseProperty("Ingredient")]
    public virtual ICollection<IngredientAlternative> IngredientAlternativeIngredients { get; set; } = new List<IngredientAlternative>();

    [InverseProperty("Ingredient")]
    public virtual ICollection<InventoryItem> InventoryItems { get; set; } = new List<InventoryItem>();

    [InverseProperty("Ingredient")]
    public virtual ICollection<PurchaseOrderLine> PurchaseOrderLines { get; set; } = new List<PurchaseOrderLine>();

    [InverseProperty("Ingredient")]
    public virtual ICollection<PurchasePlanLine> PurchasePlanLines { get; set; } = new List<PurchasePlanLine>();

    [ForeignKey("SchoolId")]
    [InverseProperty("Ingredients")]
    public virtual School School { get; set; } = null!;

    [ForeignKey("IngredientId")]
    [InverseProperty("Ingredients")]
    public virtual ICollection<FoodInFridge> Samples { get; set; } = new List<FoodInFridge>();
}
