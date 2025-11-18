using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace SMMS.Domain.Entities.rag;

[Table("MenuRecommendSessions", Schema = "rag")]
public partial class MenuRecommendSession
{
    [Key]
    public long SessionId { get; set; }

    public Guid UserId { get; set; }

    public DateTime CreatedAt { get; set; }

    public string RequestJson { get; set; } = null!;

    public int CandidateCount { get; set; }

    [StringLength(50)]
    public string? ModelVersion { get; set; }

    [InverseProperty("Session")]
    public virtual ICollection<MenuRecommendResult> MenuRecommendResults { get; set; } = new List<MenuRecommendResult>();
}
