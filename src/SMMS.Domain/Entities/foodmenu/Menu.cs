using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using SMMS.Domain.Entities.auth;
using SMMS.Domain.Entities.fridge;
using SMMS.Domain.Entities.purchasing;
using SMMS.Domain.Entities.school;

namespace SMMS.Domain.Entities.foodmenu;

[Table("Menus", Schema = "foodmenu")]
public partial class Menu
{
    [Key]
    public int MenuId { get; set; }

    public DateTime? PublishedAt { get; set; }

    public Guid SchoolId { get; set; }

    public bool IsVisible { get; set; }

    public short? WeekNo { get; set; }

    public DateTime CreatedAt { get; set; }

    public Guid? ConfirmedBy { get; set; }

    public DateTime? ConfirmedAt { get; set; }

    public bool AskToDelete { get; set; }

    public int? YearId { get; set; }

    [ForeignKey("ConfirmedBy")]
    [InverseProperty("Menus")]
    public virtual User? ConfirmedByNavigation { get; set; }

    [InverseProperty("Menu")]
    public virtual ICollection<FoodInFridge> FoodInFridges { get; set; } = new List<FoodInFridge>();

    [InverseProperty("Menu")]
    public virtual ICollection<MenuDay> MenuDays { get; set; } = new List<MenuDay>();

    [ForeignKey("SchoolId")]
    [InverseProperty("Menus")]
    public virtual School School { get; set; } = null!;

    [ForeignKey("YearId")]
    [InverseProperty("Menus")]
    public virtual AcademicYear? Year { get; set; }
}

