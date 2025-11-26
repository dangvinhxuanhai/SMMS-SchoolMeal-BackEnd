using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace SMMS.Domain.Entities.school;

[Table("StudentHealthRecords", Schema = "school")]
public partial class StudentHealthRecord
{
    [Key]
    public Guid RecordId { get; set; }

    public Guid StudentId { get; set; }

    public DateOnly RecordAt { get; set; }

    [Column(TypeName = "decimal(5, 2)")]
    public decimal? HeightCm { get; set; }

    [Column(TypeName = "decimal(5, 2)")]
    public decimal? WeightKg { get; set; }

    public int? YearId { get; set; }

    [ForeignKey("StudentId")]
    [InverseProperty("StudentHealthRecords")]
    public virtual Student Student { get; set; } = null!;

    [ForeignKey("YearId")]
    [InverseProperty("StudentHealthRecords")]
    public virtual AcademicYear? Year { get; set; }
}
