namespace SMMS.WebAPI.Controllers.Modules.KitchenStaff.Fastapi.DTOs;

public class CreateMenuFoodItemDto
{
    public int DailyMealId { get; set; }
    public int FoodId { get; set; }
    public int? SortOrder { get; set; }
}

