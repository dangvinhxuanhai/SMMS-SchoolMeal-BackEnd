using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace SMMS.Domain.Entities.school;

[Table("Classes", Schema = "school")]
[Index("TeacherId", Name = "UQ__Classes__EDF259652891263B", IsUnique = true)]
public partial class Class
{
    [Key]
    public Guid ClassId { get; set; }

    [StringLength(50)]
    public string ClassName { get; set; } = null!;

    public Guid SchoolId { get; set; }

    public int YearId { get; set; }

    public Guid? TeacherId { get; set; }

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
