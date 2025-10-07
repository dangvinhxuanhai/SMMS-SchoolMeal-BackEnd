using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace SMMS.Domain.Entities;

public partial class FoodItem
{
    [Key]
    public int FoodId { get; set; }

    [StringLength(150)]
    public string FoodName { get; set; } = null!;

    [StringLength(150)]
    public string? FoodType { get; set; }

    [StringLength(500)]
    public string? Description { get; set; }

    [StringLength(300)]
    public string? ImageUrl { get; set; }

    public bool IsActive { get; set; }

    [InverseProperty("Food")]
    public virtual ICollection<FoodItemIngredient> FoodItemIngredients { get; set; } = new List<FoodItemIngredient>();

    [InverseProperty("Food")]
    public virtual ICollection<MenuDayFoodItem> MenuDayFoodItems { get; set; } = new List<MenuDayFoodItem>();

    [InverseProperty("Food")]
    public virtual ICollection<MenuFoodItem> MenuFoodItems { get; set; } = new List<MenuFoodItem>();
}
