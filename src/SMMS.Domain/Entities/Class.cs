using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace SMMS.Domain.Entities;

[Index("TeacherId", Name = "UQ__Classes__EDF25965F0B70989", IsUnique = true)]
public partial class Class
{
    [Key]
    public int ClassId { get; set; }

    [StringLength(50)]
    public string ClassName { get; set; } = null!;

    public int SchoolId { get; set; }

    public int YearId { get; set; }

    public int? TeacherId { get; set; }

    public bool IsActive { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public int? UpdatedBy { get; set; }

    [ForeignKey("SchoolId")]
    [InverseProperty("Classes")]
    public virtual School School { get; set; } = null!;

    [InverseProperty("Class")]
    public virtual ICollection<StudentClass> StudentClasses { get; set; } = new List<StudentClass>();

    [ForeignKey("TeacherId")]
    [InverseProperty("Class")]
    public virtual Teacher? Teacher { get; set; }

    [ForeignKey("YearId")]
    [InverseProperty("Classes")]
    public virtual AcademicYear Year { get; set; } = null!;
}
