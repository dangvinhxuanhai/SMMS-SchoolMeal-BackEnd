using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using SMMS.Domain.Entities.auth;
using SMMS.Domain.Entities.nutrition;
using SMMS.Domain.Entities.school;

namespace SMMS.Domain.Entities.rag;

[PrimaryKey("RequestId", "AllergenId")]
[Table("RAG_RequestAllergens", Schema = "rag")]
[Index("RequestId", Name = "IX_RAG_RA_Request")]
public partial class RagRequestAllergen
{
    [Key]
    public Guid RequestId { get; set; }

    public Guid SchoolId { get; set; }

    public Guid CreatedBy { get; set; }

    public DateTime CreatedAt { get; set; }

    [Key]
    public int AllergenId { get; set; }

    [ForeignKey("AllergenId")]
    [InverseProperty("RagRequestAllergens")]
    public virtual Allergen Allergen { get; set; } = null!;

    [ForeignKey("CreatedBy")]
    [InverseProperty("RagRequestAllergens")]
    public virtual User CreatedByNavigation { get; set; } = null!;

    [ForeignKey("SchoolId")]
    [InverseProperty("RagRequestAllergens")]
    public virtual School School { get; set; } = null!;
}
