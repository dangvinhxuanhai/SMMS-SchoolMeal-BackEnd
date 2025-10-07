using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace SMMS.Domain.Entities;

[Index("Phone", Name = "UQ_Users_Phone", IsUnique = true)]
[Index("Email", Name = "UQ__Users__A9D105341AC560D3", IsUnique = true)]
public partial class User
{
    [Key]
    public int UserId { get; set; }

    [StringLength(255)]
    public string? Email { get; set; }

    [StringLength(255)]
    public string PasswordHash { get; set; } = null!;

    [StringLength(150)]
    public string FullName { get; set; } = null!;

    [StringLength(20)]
    public string Phone { get; set; } = null!;

    [StringLength(10)]
    public string LanguagePref { get; set; } = null!;

    [StringLength(3)]
    [Unicode(false)]
    public string RoleCode { get; set; } = null!;

    public int? SchoolId { get; set; }

    public bool IsActive { get; set; }

    public DateTime CreatedAt { get; set; }

    public int? CreatedBy { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public int? UpdatedBy { get; set; }

    [StringLength(20)]
    public string? IdentityNo { get; set; }

    [InverseProperty("NotifiedByNavigation")]
    public virtual ICollection<Attendance> Attendances { get; set; } = new List<Attendance>();

    [InverseProperty("User")]
    public virtual ICollection<AuditLog> AuditLogs { get; set; } = new List<AuditLog>();

    [InverseProperty("Sender")]
    public virtual ICollection<Feedback> Feedbacks { get; set; } = new List<Feedback>();

    [InverseProperty("UpdatedByNavigation")]
    public virtual ICollection<User> InverseUpdatedByNavigation { get; set; } = new List<User>();

    [InverseProperty("User")]
    public virtual ICollection<NotificationRecipient> NotificationRecipients { get; set; } = new List<NotificationRecipient>();

    [InverseProperty("Sender")]
    public virtual ICollection<Notification> Notifications { get; set; } = new List<Notification>();

    [InverseProperty("User")]
    public virtual ICollection<PurchaseOrderLine> PurchaseOrderLines { get; set; } = new List<PurchaseOrderLine>();

    [InverseProperty("User")]
    public virtual ICollection<PurchaseOrder> PurchaseOrders { get; set; } = new List<PurchaseOrder>();

    [InverseProperty("Staff")]
    public virtual ICollection<PurchasePlan> PurchasePlans { get; set; } = new List<PurchasePlan>();

    [InverseProperty("CreatedByNavigation")]
    public virtual ICollection<ScheduleMeal> ScheduleMeals { get; set; } = new List<ScheduleMeal>();

    [ForeignKey("SchoolId")]
    [InverseProperty("Users")]
    public virtual School? School { get; set; }

    [InverseProperty("UploadedByNavigation")]
    public virtual ICollection<StudentImage> StudentImages { get; set; } = new List<StudentImage>();

    [InverseProperty("Parent")]
    public virtual ICollection<Student> Students { get; set; } = new List<Student>();

    [InverseProperty("TeacherNavigation")]
    public virtual Teacher? Teacher { get; set; }

    [ForeignKey("UpdatedBy")]
    [InverseProperty("InverseUpdatedByNavigation")]
    public virtual User? UpdatedByNavigation { get; set; }
}
