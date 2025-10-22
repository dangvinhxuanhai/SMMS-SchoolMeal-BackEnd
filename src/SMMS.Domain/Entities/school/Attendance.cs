using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using SMMS.Domain.Entities.auth;

namespace SMMS.Domain.Entities.school;

[Table("Attendance", Schema = "school")]
public partial class Attendance
{
    [Key]
    public int AttendanceId { get; set; }

    public Guid StudentId { get; set; }

    public DateOnly AbsentDate { get; set; }

    [StringLength(300)]
    public string? Reason { get; set; }

    public Guid? NotifiedBy { get; set; }

    public DateTime CreatedAt { get; set; }

    [ForeignKey("NotifiedBy")]
    [InverseProperty("Attendances")]
    public virtual User? NotifiedByNavigation { get; set; }

    [ForeignKey("StudentId")]
    [InverseProperty("Attendances")]
    public virtual Student Student { get; set; } = null!;
}
