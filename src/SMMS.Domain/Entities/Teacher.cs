using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace SMMS.Domain.Entities;

[Index("EmployeeCode", Name = "UQ__Teachers__1F6425482A550E41", IsUnique = true)]
public partial class Teacher
{
    [Key]
    public int TeacherId { get; set; }

    [StringLength(30)]
    public string? EmployeeCode { get; set; }

    public DateOnly? HiredDate { get; set; }

    public bool IsActive { get; set; }

    [InverseProperty("Teacher")]
    public virtual Class? Class { get; set; }

    [ForeignKey("TeacherId")]
    [InverseProperty("Teacher")]
    public virtual User TeacherNavigation { get; set; } = null!;
}
