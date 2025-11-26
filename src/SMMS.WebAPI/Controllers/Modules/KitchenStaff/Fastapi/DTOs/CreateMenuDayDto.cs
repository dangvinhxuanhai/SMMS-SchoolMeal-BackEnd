namespace SMMS.WebAPI.Controllers.Modules.KitchenStaff.Fastapi.DTOs;

public class CreateMenuDayDto
{
    public int MenuId { get; set; }
    public byte DayOfWeek { get; set; } // 1=Monday to 7=Sunday
    public string MealType { get; set; } = null!;
    public string? Notes { get; set; }
}

