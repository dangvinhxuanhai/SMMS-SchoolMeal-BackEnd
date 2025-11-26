namespace SMMS.WebAPI.Controllers.Modules.KitchenStaff.Fastapi.DTOs;

public class CreateMenuDayFoodItemDto
{
    public int MenuDayId { get; set; }
    public int FoodId { get; set; }
    public int? SortOrder { get; set; }
}

