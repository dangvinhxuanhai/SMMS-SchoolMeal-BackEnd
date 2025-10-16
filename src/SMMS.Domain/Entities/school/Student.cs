using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using SMMS.Domain.Entities.auth;
using SMMS.Domain.Entities.billing;
using SMMS.Domain.Entities.nutrition;

namespace SMMS.Domain.Entities.school;

[Table("Students", Schema = "school")]
public partial class Student
{
    [Key]
    public Guid StudentId { get; set; }

    [StringLength(150)]
    public string FullName { get; set; } = null!;

    [StringLength(1)]
    [Unicode(false)]
    public string? Gender { get; set; }

    public DateOnly? DateOfBirth { get; set; }

    public Guid SchoolId { get; set; }

    public Guid? ParentId { get; set; }

    [StringLength(50)]
    public string? RelationName { get; set; }

    [StringLength(300)]
    public string? AvatarUrl { get; set; }

    public bool IsActive { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public int? UpdatedBy { get; set; }

    [InverseProperty("Student")]
    public virtual ICollection<Attendance> Attendances { get; set; } = new List<Attendance>();

    [InverseProperty("Student")]
    public virtual ICollection<Invoice> Invoices { get; set; } = new List<Invoice>();

    [ForeignKey("ParentId")]
    [InverseProperty("Students")]
    public virtual User? Parent { get; set; }

    [ForeignKey("SchoolId")]
    [InverseProperty("Students")]
    public virtual School School { get; set; } = null!;

    [InverseProperty("Student")]
    public virtual ICollection<StudentAllergen> StudentAllergens { get; set; } = new List<StudentAllergen>();

    [InverseProperty("Student")]
    public virtual ICollection<StudentClass> StudentClasses { get; set; } = new List<StudentClass>();

    [InverseProperty("Student")]
    public virtual ICollection<StudentHealthRecord> StudentHealthRecords { get; set; } = new List<StudentHealthRecord>();

    [InverseProperty("Student")]
    public virtual ICollection<StudentImage> StudentImages { get; set; } = new List<StudentImage>();
}
