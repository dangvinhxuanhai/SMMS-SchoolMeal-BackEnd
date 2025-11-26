using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace SMMS.Domain.Entities.foodmenu;

[Table("MenuDays", Schema = "foodmenu")]
[Index("MenuId", "DayOfWeek", "MealType", Name = "IX_MenuDays_Menu")]
[Index("MenuId", "DayOfWeek", "MealType", Name = "UQ_MenuDays", IsUnique = true)]
public partial class MenuDay
{
    [Key]
    public int MenuDayId { get; set; }

    public int MenuId { get; set; }

    public byte DayOfWeek { get; set; }

    [StringLength(20)]
    public string MealType { get; set; } = null!;

    [StringLength(300)]
    public string? Notes { get; set; }

    [ForeignKey("MenuId")]
    [InverseProperty("MenuDays")]
    public virtual Menu Menu { get; set; } = null!;

    [InverseProperty("MenuDay")]
    public virtual ICollection<MenuDayFoodItem> MenuDayFoodItems { get; set; } = new List<MenuDayFoodItem>();
}
