namespace SMMS.WebAPI.Controllers.Modules.KitchenStaff.Fastapi.DTOs;

public class CreateMenuDto
{
    public DateTime? PublishedAt { get; set; }
    public Guid SchoolId { get; set; }
    public bool IsVisible { get; set; } = true;
    public short? WeekNo { get; set; }
    public Guid? ConfirmedBy { get; set; }
    public DateTime? ConfirmedAt { get; set; }
    public bool AskToDelete { get; set; } = false;
    public int? YearId { get; set; }
}

