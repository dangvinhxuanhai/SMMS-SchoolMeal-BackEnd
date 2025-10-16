using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace SMMS.Domain.Entities.school;

[PrimaryKey("StudentId", "ClassId")]
[Table("StudentClasses", Schema = "school")]
public partial class StudentClass
{
    [Key]
    public Guid StudentId { get; set; }

    [Key]
    public Guid ClassId { get; set; }

    public DateOnly JoinedDate { get; set; }

    public DateOnly? LeftDate { get; set; }

    public bool RegistStatus { get; set; }

    [ForeignKey("ClassId")]
    [InverseProperty("StudentClasses")]
    public virtual Class Class { get; set; } = null!;

    [ForeignKey("StudentId")]
    [InverseProperty("StudentClasses")]
    public virtual Student Student { get; set; } = null!;
}
