using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace SMMS.Domain.Entities;

public partial class StudentImage
{
    [Key]
    public int ImageId { get; set; }

    public int StudentId { get; set; }

    public int? UploadedBy { get; set; }

    [StringLength(300)]
    public string ImageUrl { get; set; } = null!;

    [StringLength(300)]
    public string? Caption { get; set; }

    public DateTime? TakenAt { get; set; }

    public DateTime CreatedAt { get; set; }

    [ForeignKey("StudentId")]
    [InverseProperty("StudentImages")]
    public virtual Student Student { get; set; } = null!;

    [InverseProperty("Image")]
    public virtual ICollection<StudentImageTag> StudentImageTags { get; set; } = new List<StudentImageTag>();

    [ForeignKey("UploadedBy")]
    [InverseProperty("StudentImages")]
    public virtual User? UploadedByNavigation { get; set; }
}
