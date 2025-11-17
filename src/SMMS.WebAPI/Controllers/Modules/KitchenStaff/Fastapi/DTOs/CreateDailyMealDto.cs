namespace SMMS.WebAPI.Controllers.Modules.KitchenStaff.Fastapi.DTOs;

public class CreateDailyMealDto
{
    public long ScheduleMealId { get; set; }
    public DateOnly MealDate { get; set; }
    public string MealType { get; set; } = null!;
    public string? Notes { get; set; }
}

