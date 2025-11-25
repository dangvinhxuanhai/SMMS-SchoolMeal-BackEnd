using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using SMMS.Domain.Entities.auth;

namespace SMMS.Domain.Entities.school;

[Table("Tags", Schema = "school")]
[Index("TagName", Name = "UQ__Tags__BDE0FD1D1C99F83F", IsUnique = true)]
public partial class Tag
{
    [Key]
    public int TagId { get; set; }

    [StringLength(50)]
    public string TagName { get; set; } = null!;

    public Guid SchoolId { get; set; }

    public Guid? CreatedBy { get; set; }

    [ForeignKey("CreatedBy")]
    [InverseProperty("Tags")]
    public virtual User? CreatedByNavigation { get; set; }

    [ForeignKey("SchoolId")]
    [InverseProperty("Tags")]
    public virtual School School { get; set; } = null!;

    [InverseProperty("Tag")]
    public virtual ICollection<StudentImageTag> StudentImageTags { get; set; } = new List<StudentImageTag>();
}
