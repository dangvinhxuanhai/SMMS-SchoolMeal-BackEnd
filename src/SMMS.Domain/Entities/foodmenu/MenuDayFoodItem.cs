using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using SMMS.Domain.Entities.nutrition;

namespace SMMS.Domain.Entities.foodmenu;

[PrimaryKey("MenuDayId", "FoodId")]
[Table("MenuDayFoodItems", Schema = "foodmenu")]
public partial class MenuDayFoodItem
{
    [Key]
    public int MenuDayId { get; set; }

    [Key]
    public int FoodId { get; set; }

    public int? SortOrder { get; set; }

    [ForeignKey("FoodId")]
    [InverseProperty("MenuDayFoodItems")]
    public virtual FoodItem Food { get; set; } = null!;

    [ForeignKey("MenuDayId")]
    [InverseProperty("MenuDayFoodItems")]
    public virtual MenuDay MenuDay { get; set; } = null!;
}
