using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace SMMS.Domain.Entities;

[Index("YearName", Name = "UQ__Academic__294C4DA9322796A8", IsUnique = true)]
public partial class AcademicYear
{
    [Key]
    public int YearId { get; set; }

    [StringLength(20)]
    public string YearName { get; set; } = null!;

    public DateTime? BoardingStartDate { get; set; }

    public DateTime? BoardingEndDate { get; set; }

    [InverseProperty("Year")]
    public virtual ICollection<Class> Classes { get; set; } = new List<Class>();
}
