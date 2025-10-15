using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using SMMS.Domain.Models.auth;
using SMMS.Domain.Models.foodmenu;
using SMMS.Domain.Models.fridge;
using SMMS.Domain.Models.school;

namespace SMMS.Domain.Models.nutrition;

[Table("FoodItems", Schema = "nutrition")]
public partial class FoodItem
{
    [Key]
    public int FoodId { get; set; }

    [StringLength(150)]
    public string FoodName { get; set; } = null!;

    [StringLength(150)]
    public string? FoodType { get; set; }

    [StringLength(500)]
    public string? FoodDesc { get; set; }

    [StringLength(300)]
    public string? ImageUrl { get; set; }

    public Guid? CreatedBy { get; set; }

    public DateTime CreatedAt { get; set; }

    public Guid SchoolId { get; set; }

    public bool IsActive { get; set; }

    [ForeignKey("CreatedBy")]
    [InverseProperty("FoodItems")]
    public virtual User? CreatedByNavigation { get; set; }

    [InverseProperty("Food")]
    public virtual ICollection<FoodInFridge> FoodInFridges { get; set; } = new List<FoodInFridge>();

    [InverseProperty("Food")]
    public virtual ICollection<FoodItemIngredient> FoodItemIngredients { get; set; } = new List<FoodItemIngredient>();

    [InverseProperty("Food")]
    public virtual ICollection<MenuDayFoodItem> MenuDayFoodItems { get; set; } = new List<MenuDayFoodItem>();

    [InverseProperty("Food")]
    public virtual ICollection<MenuFoodItem> MenuFoodItems { get; set; } = new List<MenuFoodItem>();

    [ForeignKey("SchoolId")]
    [InverseProperty("FoodItems")]
    public virtual School School { get; set; } = null!;
}
