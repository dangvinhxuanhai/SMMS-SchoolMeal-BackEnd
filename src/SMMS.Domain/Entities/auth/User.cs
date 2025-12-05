using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using SMMS.Domain.Entities.billing;
using SMMS.Domain.Entities.foodmenu;
using SMMS.Domain.Entities.fridge;
using SMMS.Domain.Entities.inventory;
using SMMS.Domain.Entities.Logs;
using SMMS.Domain.Entities.nutrition;
using SMMS.Domain.Entities.purchasing;
using SMMS.Domain.Entities.school;

namespace SMMS.Domain.Entities.auth;

[Table("Users", Schema = "auth")]
[Index("Phone", Name = "UQ_Users_Phone", IsUnique = true)]
public partial class User
{
    [Key]
    public Guid UserId { get; set; }

    [StringLength(255)]
    public string? Email { get; set; }

    [StringLength(255)]
    public string PasswordHash { get; set; } = null!;

    [StringLength(150)]
    public string FullName { get; set; } = null!;

    [StringLength(20)]
    public string Phone { get; set; } = null!;

    [StringLength(20)]
    public string? EmergencyPhone { get; set; }

    [StringLength(300)]
    public string? AvatarUrl { get; set; }

    [StringLength(10)]
    public string LanguagePref { get; set; } = null!;

    public int RoleId { get; set; }

    public Guid? SchoolId { get; set; }

    public bool IsActive { get; set; }

    public DateTime CreatedAt { get; set; }

    public Guid? CreatedBy { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public Guid? UpdatedBy { get; set; }

    [StringLength(20)]
    public string? IdentityNo { get; set; }

    public DateOnly? DateOfBirth { get; set; }

    public int AccessFailedCount { get; set; }

    public DateTime? LockoutEndAt { get; set; }

    public bool LockoutEnabled { get; set; }

    public bool Gender { get; set; }

    [InverseProperty("CreatedByNavigation")]
    public virtual ICollection<Allergen> Allergens { get; set; } = new List<Allergen>();

    [InverseProperty("NotifiedByNavigation")]
    public virtual ICollection<Attendance> Attendances { get; set; } = new List<Attendance>();

    [InverseProperty("User")]
    public virtual ICollection<AuditLog> AuditLogs { get; set; } = new List<AuditLog>();

    [InverseProperty("Sender")]
    public virtual ICollection<Feedback> Feedbacks { get; set; } = new List<Feedback>();

    [InverseProperty("DeletedByNavigation")]
    public virtual ICollection<FoodInFridge> FoodInFridgeDeletedByNavigations { get; set; } = new List<FoodInFridge>();

    [InverseProperty("StoredByNavigation")]
    public virtual ICollection<FoodInFridge> FoodInFridgeStoredByNavigations { get; set; } = new List<FoodInFridge>();

    [InverseProperty("CreatedByNavigation")]
    public virtual ICollection<FoodItem> FoodItems { get; set; } = new List<FoodItem>();

    [InverseProperty("CreatedByNavigation")]
    public virtual ICollection<IngredientAlternative> IngredientAlternatives { get; set; } = new List<IngredientAlternative>();

    [InverseProperty("CreatedByNavigation")]
    public virtual ICollection<Ingredient> Ingredients { get; set; } = new List<Ingredient>();

    [InverseProperty("CreatedByNavigation")]
    public virtual ICollection<InventoryItem> InventoryItems { get; set; } = new List<InventoryItem>();

    [InverseProperty("UpdatedByNavigation")]
    public virtual ICollection<User> InverseUpdatedByNavigation { get; set; } = new List<User>();

    [InverseProperty("ConfirmedByNavigation")]
    public virtual ICollection<Menu> Menus { get; set; } = new List<Menu>();

    [InverseProperty("User")]
    public virtual ICollection<NotificationRecipient> NotificationRecipients { get; set; } = new List<NotificationRecipient>();

    [InverseProperty("Sender")]
    public virtual ICollection<Notification> Notifications { get; set; } = new List<Notification>();

    [InverseProperty("User")]
    public virtual ICollection<PurchaseOrderLine> PurchaseOrderLines { get; set; } = new List<PurchaseOrderLine>();

    [InverseProperty("StaffInChargedNavigation")]
    public virtual ICollection<PurchaseOrder> PurchaseOrders { get; set; } = new List<PurchaseOrder>();

    [InverseProperty("ConfirmedByNavigation")]
    public virtual ICollection<PurchasePlan> PurchasePlanConfirmedByNavigations { get; set; } = new List<PurchasePlan>();

    [InverseProperty("Staff")]
    public virtual ICollection<PurchasePlan> PurchasePlanStaffs { get; set; } = new List<PurchasePlan>();

    [InverseProperty("User")]
    public virtual ICollection<RefreshToken> RefreshTokens { get; set; } = new List<RefreshToken>();

    [ForeignKey("RoleId")]
    [InverseProperty("Users")]
    public virtual Role Role { get; set; } = null!;

    [InverseProperty("CreatedByNavigation")]
    public virtual ICollection<ScheduleMeal> ScheduleMeals { get; set; } = new List<ScheduleMeal>();

    [ForeignKey("SchoolId")]
    [InverseProperty("Users")]
    public virtual School? School { get; set; }

    [InverseProperty("CreatedByNavigation")]
    public virtual ICollection<SchoolPaymentGateway> SchoolPaymentGatewayCreatedByNavigations { get; set; } = new List<SchoolPaymentGateway>();

    [InverseProperty("UpdatedByNavigation")]
    public virtual ICollection<SchoolPaymentGateway> SchoolPaymentGatewayUpdatedByNavigations { get; set; } = new List<SchoolPaymentGateway>();

    [InverseProperty("CreatedByNavigation")]
    public virtual ICollection<SchoolRevenue> SchoolRevenueCreatedByNavigations { get; set; } = new List<SchoolRevenue>();

    [InverseProperty("UpdatedByNavigation")]
    public virtual ICollection<SchoolRevenue> SchoolRevenueUpdatedByNavigations { get; set; } = new List<SchoolRevenue>();

    [InverseProperty("UploadedByNavigation")]
    public virtual ICollection<StudentImage> StudentImages { get; set; } = new List<StudentImage>();

    [InverseProperty("Parent")]
    public virtual ICollection<Student> Students { get; set; } = new List<Student>();

    [InverseProperty("CreatedByNavigation")]
    public virtual ICollection<Tag> Tags { get; set; } = new List<Tag>();

    [InverseProperty("TeacherNavigation")]
    public virtual Teacher? Teacher { get; set; }

    [ForeignKey("UpdatedBy")]
    [InverseProperty("InverseUpdatedByNavigation")]
    public virtual User? UpdatedByNavigation { get; set; }

    [InverseProperty("User")]
    public virtual ICollection<UserExternalLogin> UserExternalLogins { get; set; } = new List<UserExternalLogin>();
}
