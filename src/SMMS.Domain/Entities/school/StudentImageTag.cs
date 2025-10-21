using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace SMMS.Domain.Entities.school;

[PrimaryKey("ImageId", "TagId")]
[Table("StudentImageTags", Schema = "school")]
public partial class StudentImageTag
{
    [Key]
    public Guid ImageId { get; set; }

    [Key]
    public int TagId { get; set; }

    [StringLength(255)]
    public string? TagNotes { get; set; }

    public bool IsFavourite { get; set; }

    [ForeignKey("ImageId")]
    [InverseProperty("StudentImageTags")]
    public virtual StudentImage Image { get; set; } = null!;

    [ForeignKey("TagId")]
    [InverseProperty("StudentImageTags")]
    public virtual Tag Tag { get; set; } = null!;
}
