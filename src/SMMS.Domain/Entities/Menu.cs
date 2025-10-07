using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace SMMS.Domain.Entities;

public partial class Menu
{
    [Key]
    public int MenuId { get; set; }

    public int SchoolId { get; set; }

    public DateTime? PublishedAt { get; set; }

    [InverseProperty("Menu")]
    public virtual ICollection<MenuDay> MenuDays { get; set; } = new List<MenuDay>();

    [InverseProperty("Menu")]
    public virtual ICollection<PurchasePlan> PurchasePlans { get; set; } = new List<PurchasePlan>();

    [InverseProperty("Menu")]
    public virtual ICollection<ScheduleMeal> ScheduleMeals { get; set; } = new List<ScheduleMeal>();

    [ForeignKey("SchoolId")]
    [InverseProperty("Menus")]
    public virtual School School { get; set; } = null!;
}
