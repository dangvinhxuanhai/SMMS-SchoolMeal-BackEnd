using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace SMMS.Domain.Entities;

public partial class School
{
    [Key]
    public int SchoolId { get; set; }

    [StringLength(150)]
    public string SchoolName { get; set; } = null!;

    [StringLength(150)]
    public string? ContactEmail { get; set; }

    [StringLength(20)]
    public string? Hotline { get; set; }

    [StringLength(200)]
    public string? SchoolAddress { get; set; }

    public bool IsActive { get; set; }

    public DateTime CreatedAt { get; set; }

    public int? CreatedBy { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public int? UpdatedBy { get; set; }

    [InverseProperty("School")]
    public virtual ICollection<Class> Classes { get; set; } = new List<Class>();

    [InverseProperty("School")]
    public virtual ICollection<InventoryItem> InventoryItems { get; set; } = new List<InventoryItem>();

    [InverseProperty("School")]
    public virtual ICollection<Menu> Menus { get; set; } = new List<Menu>();

    [InverseProperty("School")]
    public virtual ICollection<PurchaseOrder> PurchaseOrders { get; set; } = new List<PurchaseOrder>();

    [InverseProperty("School")]
    public virtual ICollection<ScheduleMeal> ScheduleMeals { get; set; } = new List<ScheduleMeal>();

    [InverseProperty("School")]
    public virtual ICollection<Student> Students { get; set; } = new List<Student>();

    [InverseProperty("School")]
    public virtual ICollection<User> Users { get; set; } = new List<User>();
}
