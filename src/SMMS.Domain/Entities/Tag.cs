using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace SMMS.Domain.Entities;

[Index("TagName", Name = "UQ__Tags__BDE0FD1D5AA08F17", IsUnique = true)]
public partial class Tag
{
    [Key]
    public int TagId { get; set; }

    [StringLength(50)]
    public string TagName { get; set; } = null!;

    [InverseProperty("Tag")]
    public virtual ICollection<StudentImageTag> StudentImageTags { get; set; } = new List<StudentImageTag>();
}
