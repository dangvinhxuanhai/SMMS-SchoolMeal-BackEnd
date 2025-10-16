using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using SMMS.Domain.Entities.auth;
using SMMS.Domain.Entities.school;

namespace SMMS.Domain.Entities.rag;

[PrimaryKey("SuggestedFoodItemId", "SuggestedIngredientId")]
[Table("RAG_SuggestedFoodItemIngredients", Schema = "rag")]
[Index("RequestId", Name = "IX_RAG_SFII_Request")]
[Index("SchoolId", "CreatedAt", Name = "IX_RAG_SFII_School")]
public partial class RagSuggestedFoodItemIngredient
{
    [Key]
    public Guid SuggestedFoodItemId { get; set; }

    [Key]
    public Guid SuggestedIngredientId { get; set; }

    public Guid RequestId { get; set; }

    public Guid SchoolId { get; set; }

    public Guid CreatedBy { get; set; }

    public DateTime CreatedAt { get; set; }

    [Column(TypeName = "decimal(9, 2)")]
    public decimal? QuantityGram { get; set; }

    [ForeignKey("CreatedBy")]
    [InverseProperty("RagSuggestedFoodItemIngredients")]
    public virtual User CreatedByNavigation { get; set; } = null!;

    [ForeignKey("SchoolId")]
    [InverseProperty("RagSuggestedFoodItemIngredients")]
    public virtual School School { get; set; } = null!;

    [ForeignKey("SuggestedFoodItemId")]
    [InverseProperty("RagSuggestedFoodItemIngredients")]
    public virtual RagSuggestedFoodItem SuggestedFoodItem { get; set; } = null!;

    [ForeignKey("SuggestedIngredientId")]
    [InverseProperty("RagSuggestedFoodItemIngredients")]
    public virtual RagSuggestedIngredient SuggestedIngredient { get; set; } = null!;
}
