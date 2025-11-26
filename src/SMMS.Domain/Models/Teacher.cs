using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace SMMS.Domain.Models;

[Table("Teachers", Schema = "school")]
[Index("EmployeeCode", Name = "UQ__Teachers__1F6425485D46FE08", IsUnique = true)]
public partial class Teacher
{
    [Key]
    public Guid TeacherId { get; set; }

    [StringLength(100)]
    public string? EmployeeCode { get; set; }

    public DateOnly? HiredDate { get; set; }

    public bool IsActive { get; set; }

    [InverseProperty("Teacher")]
    public virtual Class? Class { get; set; }

    [ForeignKey("TeacherId")]
    [InverseProperty("Teacher")]
    public virtual User TeacherNavigation { get; set; } = null!;
}
