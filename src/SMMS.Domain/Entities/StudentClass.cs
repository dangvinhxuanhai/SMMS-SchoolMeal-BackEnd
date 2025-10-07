using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace SMMS.Domain.Entities;

[PrimaryKey("StudentId", "ClassId")]
public partial class StudentClass
{
    [Key]
    public int StudentId { get; set; }

    [Key]
    public int ClassId { get; set; }

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
