using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace SMMS.Domain.Entities.school;

[Table("StagingStudents", Schema = "school")]
public partial class StagingStudent
{
    [Key]
    public long StageId { get; set; }

    public Guid SchoolId { get; set; }

    [StringLength(150)]
    public string FullName { get; set; } = null!;

    public DateOnly DateOfBirth { get; set; }

    [StringLength(1)]
    [Unicode(false)]
    public string? Gender { get; set; }

    [StringLength(50)]
    public string? ClassName { get; set; }

    [StringLength(255)]
    public string? ParentEmail1 { get; set; }

    [StringLength(20)]
    public string? ParentRelation1 { get; set; }

    [StringLength(255)]
    public string? ParentEmail2 { get; set; }

    [StringLength(20)]
    public string? ParentRelation2 { get; set; }

    [StringLength(20)]
    public string? Phone { get; set; }

    public Guid ImportBatchId { get; set; }

    [StringLength(20)]
    public string? RowStatus { get; set; }

    [StringLength(1000)]
    public string? RowErrors { get; set; }
}
