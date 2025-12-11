using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SMMS.Application.Features.foodmenu.DTOs;
public class FeedbackKsDto
{
    public int FeedbackId { get; set; }
    public Guid SenderId { get; set; }
    public string TargetType { get; set; } = default!;
    public string? TargetRef { get; set; }
    public string Content { get; set; } = default!;
    public DateTime CreatedAt { get; set; }
    public int? DailyMealId { get; set; }

    // ✨ NEW: Rating 1–5 sao (nullable)
    public byte? Rating { get; set; }
}
