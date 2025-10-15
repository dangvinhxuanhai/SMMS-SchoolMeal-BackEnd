using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using SMMS.Domain.Models.auth;
using SMMS.Domain.Models.school;

namespace SMMS.Domain.Models.foodmenu;

[Table("ScheduleMeal", Schema = "foodmenu")]
[Index("MenuId", Name = "IX_ScheduleMeal_Menu")]
[Index("SchoolId", "WeekStart", "WeekEnd", Name = "IX_ScheduleMeal_SchoolWeek")]
[Index("SchoolId", "WeekNo", "YearNo", Name = "UQ_ScheduleMeal_School_WeekNoYear", IsUnique = true)]
[Index("SchoolId", "WeekStart", Name = "UQ_ScheduleMeal_School_WeekStart", IsUnique = true)]
public partial class ScheduleMeal
{
    [Key]
    public long ScheduleMealId { get; set; }

    public Guid SchoolId { get; set; }

    public int MenuId { get; set; }

    public DateOnly WeekStart { get; set; }

    public DateOnly WeekEnd { get; set; }

    public short WeekNo { get; set; }

    public short YearNo { get; set; }

    [StringLength(20)]
    public string Status { get; set; } = null!;

    public DateTime? PublishedAt { get; set; }

    [StringLength(300)]
    public string? Notes { get; set; }

    public DateTime CreatedAt { get; set; }

    public Guid? CreatedBy { get; set; }

    [ForeignKey("CreatedBy")]
    [InverseProperty("ScheduleMeals")]
    public virtual User? CreatedByNavigation { get; set; }

    [InverseProperty("ScheduleMeal")]
    public virtual ICollection<DailyMeal> DailyMeals { get; set; } = new List<DailyMeal>();

    [ForeignKey("MenuId")]
    [InverseProperty("ScheduleMeals")]
    public virtual Menu Menu { get; set; } = null!;

    [ForeignKey("SchoolId")]
    [InverseProperty("ScheduleMeals")]
    public virtual School School { get; set; } = null!;
}
