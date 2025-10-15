using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using SMMS.Domain.Models.auth;
using SMMS.Domain.Models.school;

namespace SMMS.Domain.Models.rag;

[Table("RAG_RequestInput", Schema = "rag")]
[Index("RequestId", Name = "IX_RAG_RI_Request")]
[Index("SchoolId", "CreatedAt", Name = "IX_RAG_RI_School")]
public partial class RagRequestInput
{
    [Key]
    public Guid RagInputId { get; set; }

    public Guid RequestId { get; set; }

    public Guid SchoolId { get; set; }

    public Guid CreatedBy { get; set; }

    public DateTime CreatedAt { get; set; }

    public bool IsHeader { get; set; }

    public string? PromptText { get; set; }

    public int? MaxSuggestions { get; set; }

    public int? SourceIngredientId { get; set; }

    [StringLength(150)]
    public string? IngredientName { get; set; }

    [StringLength(300)]
    public string? Notes { get; set; }

    [ForeignKey("CreatedBy")]
    [InverseProperty("RagRequestInputs")]
    public virtual User CreatedByNavigation { get; set; } = null!;

    [ForeignKey("SchoolId")]
    [InverseProperty("RagRequestInputs")]
    public virtual School School { get; set; } = null!;
}
