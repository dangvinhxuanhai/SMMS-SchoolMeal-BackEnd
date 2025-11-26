using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using SMMS.Domain.Entities.nutrition;

namespace SMMS.Domain.Entities.rag;

[PrimaryKey("SessionId", "FoodId", "IsMain")]
[Table("MenuRecommendResults", Schema = "rag")]
public partial class MenuRecommendResult
{
    [Key]
    public long SessionId { get; set; }

    [Key]
    public int FoodId { get; set; }

    [Key]
    public bool IsMain { get; set; }

    public int RankShown { get; set; }

    public double Score { get; set; }

    public bool IsChosen { get; set; }

    public DateTime? ChosenAt { get; set; }

    [ForeignKey("FoodId")]
    [InverseProperty("MenuRecommendResults")]
    public virtual FoodItem Food { get; set; } = null!;

    [ForeignKey("SessionId")]
    [InverseProperty("MenuRecommendResults")]
    public virtual MenuRecommendSession Session { get; set; } = null!;
}
