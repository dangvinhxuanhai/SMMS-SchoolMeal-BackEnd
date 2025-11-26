using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using SMMS.Domain.Entities.auth;

namespace SMMS.Domain.Entities.school;

[Table("StudentImages", Schema = "school")]
public partial class StudentImage
{
    [Key]
    public Guid ImageId { get; set; }

    public Guid StudentId { get; set; }

    public Guid? UploadedBy { get; set; }

    [StringLength(300)]
    public string ImageUrl { get; set; } = null!;

    [StringLength(300)]
    public string? Caption { get; set; }

    public DateTime? TakenAt { get; set; }

    public int? YearId { get; set; }

    public DateTime CreatedAt { get; set; }

    [ForeignKey("StudentId")]
    [InverseProperty("StudentImages")]
    public virtual Student Student { get; set; } = null!;

    [InverseProperty("Image")]
    public virtual ICollection<StudentImageTag> StudentImageTags { get; set; } = new List<StudentImageTag>();

    [ForeignKey("UploadedBy")]
    [InverseProperty("StudentImages")]
    public virtual User? UploadedByNavigation { get; set; }

    [ForeignKey("YearId")]
    [InverseProperty("StudentImages")]
    public virtual AcademicYear? Year { get; set; }
}
