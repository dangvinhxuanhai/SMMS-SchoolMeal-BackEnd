using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using SMMS.Domain.Entities.auth;
using SMMS.Domain.Entities.billing;
using SMMS.Domain.Entities.foodmenu;
using SMMS.Domain.Entities.fridge;
using SMMS.Domain.Entities.inventory;
using SMMS.Domain.Entities.nutrition;
using SMMS.Domain.Entities.purchasing;

namespace SMMS.Domain.Entities.school;

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

    [StringLength(200)]
    public string? SchoolAddress { get; set; }

    public bool IsActive { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    [StringLength(30)]
    public string? SettlementAccountNo { get; set; }

    [StringLength(10)]
    public string? SettlementBankCode { get; set; }

    [StringLength(100)]
    public string? SettlementAccountName { get; set; }

    public Guid? CreatedBy { get; set; }

    public Guid? UpdatedBy { get; set; }

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
    public virtual ICollection<ScheduleMeal> ScheduleMeals { get; set; } = new List<ScheduleMeal>();

    [InverseProperty("School")]
    public virtual ICollection<SchoolPaymentGateway> SchoolPaymentGateways { get; set; } = new List<SchoolPaymentGateway>();

    [InverseProperty("School")]
    public virtual ICollection<SchoolPaymentSetting> SchoolPaymentSettings { get; set; } = new List<SchoolPaymentSetting>();

    [InverseProperty("School")]
    public virtual ICollection<SchoolRevenue> SchoolRevenues { get; set; } = new List<SchoolRevenue>();

    [InverseProperty("School")]
    public virtual ICollection<Student> Students { get; set; } = new List<Student>();

    [InverseProperty("School")]
    public virtual ICollection<Tag> Tags { get; set; } = new List<Tag>();

    [InverseProperty("School")]
    public virtual ICollection<User> Users { get; set; } = new List<User>();
}
