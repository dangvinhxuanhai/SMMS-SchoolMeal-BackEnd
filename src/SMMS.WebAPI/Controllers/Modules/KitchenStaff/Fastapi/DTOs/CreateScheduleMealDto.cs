namespace SMMS.WebAPI.Controllers.Modules.KitchenStaff.Fastapi.DTOs;

public class CreateScheduleMealDto
{
    public Guid SchoolId { get; set; }
    public int MenuId { get; set; }
    public DateOnly WeekStart { get; set; }
    public DateOnly WeekEnd { get; set; }
    public short WeekNo { get; set; }
    public short YearNo { get; set; }
    public string Status { get; set; } = "Draft"; // 'Draft' | 'Published' | 'Archived' ~ có thể fix lại db -> có thể là đổi lại active or not???
    public DateTime? PublishedAt { get; set; }
    public string? Notes { get; set; }
    public Guid? CreatedBy { get; set; }
}

