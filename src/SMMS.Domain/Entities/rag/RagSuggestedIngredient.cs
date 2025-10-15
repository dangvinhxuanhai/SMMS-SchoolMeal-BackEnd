using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using SMMS.Domain.Models.auth;
using SMMS.Domain.Models.nutrition;
using SMMS.Domain.Models.school;

namespace SMMS.Domain.Models.rag;

[Table("RAG_SuggestedIngredients", Schema = "rag")]
[Index("RequestId", Name = "IX_RAG_SI_Request")]
[Index("SchoolId", "CreatedAt", Name = "IX_RAG_SI_School")]
public partial class RagSuggestedIngredient
{
    [Key]
    public Guid SuggestedIngredientId { get; set; }

    public Guid RequestId { get; set; }

    public Guid SchoolId { get; set; }

    public Guid CreatedBy { get; set; }

    public DateTime CreatedAt { get; set; }

    public int? SourceIngredientId { get; set; }

    [StringLength(150)]
    public string? IngredientName { get; set; }

    [StringLength(300)]
    public string? Notes { get; set; }

    [ForeignKey("CreatedBy")]
    [InverseProperty("RagSuggestedIngredients")]
    public virtual User CreatedByNavigation { get; set; } = null!;

    [InverseProperty("SuggestedIngredient")]
    public virtual ICollection<RagSuggestedFoodItemIngredient> RagSuggestedFoodItemIngredients { get; set; } = new List<RagSuggestedFoodItemIngredient>();

    [ForeignKey("SchoolId")]
    [InverseProperty("RagSuggestedIngredients")]
    public virtual School School { get; set; } = null!;

    [ForeignKey("SourceIngredientId")]
    [InverseProperty("RagSuggestedIngredients")]
    public virtual Ingredient? SourceIngredient { get; set; }
}
