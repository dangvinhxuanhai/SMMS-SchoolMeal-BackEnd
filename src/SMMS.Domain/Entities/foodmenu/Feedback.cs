using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using SMMS.Domain.Entities.auth;

namespace SMMS.Domain.Entities.foodmenu;

[Table("Feedbacks", Schema = "foodmenu")]
public partial class Feedback
{
    [Key]
    public int FeedbackId { get; set; }

    public Guid SenderId { get; set; }

    [StringLength(20)]
    public string TargetType { get; set; } = null!;

    [StringLength(50)]
    public string? TargetRef { get; set; }

    [StringLength(1000)]
    public string Content { get; set; } = null!;

    public DateTime CreatedAt { get; set; }

    public int? DailyMealId { get; set; }

    public byte? Rating { get; set; }

    [ForeignKey("DailyMealId")]
    [InverseProperty("Feedbacks")]
    public virtual DailyMeal? DailyMeal { get; set; }

    [ForeignKey("SenderId")]
    [InverseProperty("Feedbacks")]
    public virtual User Sender { get; set; } = null!;
}
