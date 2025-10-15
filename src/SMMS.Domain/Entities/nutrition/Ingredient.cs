using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using SMMS.Domain.Models.auth;
using SMMS.Domain.Models.fridge;
using SMMS.Domain.Models.inventory;
using SMMS.Domain.Models.purchasing;
using SMMS.Domain.Models.rag;
using SMMS.Domain.Models.school;

namespace SMMS.Domain.Models.nutrition;

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

    [InverseProperty("SourceIngredient")]
    public virtual ICollection<RagSuggestedIngredient> RagSuggestedIngredients { get; set; } = new List<RagSuggestedIngredient>();

    [ForeignKey("SchoolId")]
    [InverseProperty("Ingredients")]
    public virtual School School { get; set; } = null!;

    [ForeignKey("IngredientId")]
    [InverseProperty("Ingredients")]
    public virtual ICollection<FoodInFridge> Samples { get; set; } = new List<FoodInFridge>();
}
