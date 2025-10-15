using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using SMMS.Domain.Models.auth;
using SMMS.Domain.Models.foodmenu;
using SMMS.Domain.Models.fridge;
using SMMS.Domain.Models.inventory;
using SMMS.Domain.Models.nutrition;
using SMMS.Domain.Models.purchasing;
using SMMS.Domain.Models.rag;

namespace SMMS.Domain.Models.school;

[Table("Schools", Schema = "school")]
public partial class School
{
    [Key]
    public Guid SchoolId { get; set; }

    [StringLength(150)]
    public string SchoolName { get; set; } = null!;

    [StringLength(150)]
    public string? ContactEmail { get; set; }

    [StringLength(20)]
    public string? Hotline { get; set; }

    public string? SchoolContract { get; set; }

    [StringLength(200)]
    public string? SchoolAddress { get; set; }

    public bool IsActive { get; set; }

    public DateTime CreatedAt { get; set; }

    public int? CreatedBy { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public int? UpdatedBy { get; set; }

    [InverseProperty("School")]
    public virtual ICollection<AcademicYear> AcademicYears { get; set; } = new List<AcademicYear>();

    [InverseProperty("School")]
    public virtual ICollection<Allergen> Allergens { get; set; } = new List<Allergen>();

    [InverseProperty("School")]
    public virtual ICollection<Class> Classes { get; set; } = new List<Class>();

    [InverseProperty("School")]
    public virtual ICollection<FoodInFridge> FoodInFridges { get; set; } = new List<FoodInFridge>();

    [InverseProperty("School")]
    public virtual ICollection<FoodItem> FoodItems { get; set; } = new List<FoodItem>();

    [InverseProperty("School")]
    public virtual ICollection<Ingredient> Ingredients { get; set; } = new List<Ingredient>();

    [InverseProperty("School")]
    public virtual ICollection<InventoryItem> InventoryItems { get; set; } = new List<InventoryItem>();

    [InverseProperty("School")]
    public virtual ICollection<Menu> Menus { get; set; } = new List<Menu>();

    [InverseProperty("School")]
    public virtual ICollection<PurchaseOrder> PurchaseOrders { get; set; } = new List<PurchaseOrder>();

    [InverseProperty("School")]
    public virtual ICollection<RagRequestAllergen> RagRequestAllergens { get; set; } = new List<RagRequestAllergen>();

    [InverseProperty("School")]
    public virtual ICollection<RagRequestInput> RagRequestInputs { get; set; } = new List<RagRequestInput>();

    [InverseProperty("School")]
    public virtual ICollection<RagSuggestedFoodItemIngredient> RagSuggestedFoodItemIngredients { get; set; } = new List<RagSuggestedFoodItemIngredient>();

    [InverseProperty("School")]
    public virtual ICollection<RagSuggestedFoodItem> RagSuggestedFoodItems { get; set; } = new List<RagSuggestedFoodItem>();

    [InverseProperty("School")]
    public virtual ICollection<RagSuggestedIngredient> RagSuggestedIngredients { get; set; } = new List<RagSuggestedIngredient>();

    [InverseProperty("School")]
    public virtual ICollection<ScheduleMeal> ScheduleMeals { get; set; } = new List<ScheduleMeal>();

    [InverseProperty("School")]
    public virtual ICollection<Student> Students { get; set; } = new List<Student>();

    [InverseProperty("School")]
    public virtual ICollection<Tag> Tags { get; set; } = new List<Tag>();

    [InverseProperty("School")]
    public virtual ICollection<User> Users { get; set; } = new List<User>();
}
