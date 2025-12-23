using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using SMMS.Domain.Entities.auth;

namespace SMMS.Domain.Entities.foodmenu;

[Table("DailyMealEvidences", Schema = "foodmenu")]
[Index("DailyMealId", Name = "IX_MealEvidences_Meal")]
public partial class DailyMealEvidence
{
    [Key]
    public long EvidenceId { get; set; }

    public int DailyMealId { get; set; }

    [StringLength(500)]
    public string EvidenceUrl { get; set; } = null!;

    [StringLength(200)]
    public string? Caption { get; set; }

    public DateTime? UploadedAt { get; set; }

    public Guid? UploadedBy { get; set; }

    [ForeignKey("DailyMealId")]
    [InverseProperty("DailyMealEvidences")]
    public virtual DailyMeal DailyMeal { get; set; } = null!;

    [ForeignKey("UploadedBy")]
    [InverseProperty("DailyMealEvidences")]
    public virtual User? UploadedByNavigation { get; set; }
}
