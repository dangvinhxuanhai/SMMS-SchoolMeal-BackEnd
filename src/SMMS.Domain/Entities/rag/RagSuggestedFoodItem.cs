using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using SMMS.Domain.Entities.auth;
using SMMS.Domain.Entities.school;

namespace SMMS.Domain.Entities.rag;

[Table("RAG_SuggestedFoodItems", Schema = "rag")]
[Index("RequestId", Name = "IX_RAG_SF_Request")]
[Index("SchoolId", "CreatedAt", Name = "IX_RAG_SF_School")]
public partial class RagSuggestedFoodItem
{
    [Key]
    public Guid SuggestedFoodItemId { get; set; }

    public Guid RequestId { get; set; }

    public Guid SchoolId { get; set; }

    public Guid CreatedBy { get; set; }

    public DateTime CreatedAt { get; set; }

    [StringLength(200)]
    public string FoodName { get; set; } = null!;

    [StringLength(1000)]
    public string? Description { get; set; }

    [StringLength(300)]
    public string? ImageUrl { get; set; }

    [Column(TypeName = "decimal(4, 3)")]
    public decimal? ConfidenceScore { get; set; }

    [ForeignKey("CreatedBy")]
    [InverseProperty("RagSuggestedFoodItems")]
    public virtual User CreatedByNavigation { get; set; } = null!;

    [InverseProperty("SuggestedFoodItem")]
    public virtual ICollection<RagSuggestedFoodItemIngredient> RagSuggestedFoodItemIngredients { get; set; } = new List<RagSuggestedFoodItemIngredient>();

    [ForeignKey("SchoolId")]
    [InverseProperty("RagSuggestedFoodItems")]
    public virtual School School { get; set; } = null!;
}
