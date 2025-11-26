using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SMMS.Application.Features.Wardens.DTOs;

public class FeedbackDto
{
    public int FeedbackId { get; set; }
    public string Title { get; set; } = string.Empty; // 🆕 [ClassName] + [TeacherName] + [Date]
    public string SenderName { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public string? TargetType { get; set; }  // “kitchen” / “parents”
    public string? TargetRef { get; set; }                 // studentName
    public DateTime CreatedAt { get; set; }
    public int? DailyMealId { get; set; }
}

public class CreateFeedbackRequest
{
    public Guid SenderId { get; set; }          // Giám thị gửi phản hồi
    public string TargetType { get; set; } // hoặc “parents”
    public string? TargetRef { get; set; }      // Tên học sinh (nếu gửi tới phụ huynh)
    public string Content { get; set; } = null!;
    public int? DailyMealId { get; set; }       // Bữa ăn phản hồi (nếu có)
}
