using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace SMMS.Domain.Entities;

public partial class StudentHealthRecord
{
    [Key]
    public int RecordId { get; set; }

    public int StudentId { get; set; }

    public DateOnly RecordMonth { get; set; }

    [Column(TypeName = "decimal(5, 2)")]
    public decimal? HeightCm { get; set; }

    [Column(TypeName = "decimal(5, 2)")]
    public decimal? WeightKg { get; set; }

    [ForeignKey("StudentId")]
    [InverseProperty("StudentHealthRecords")]
    public virtual Student Student { get; set; } = null!;
}
