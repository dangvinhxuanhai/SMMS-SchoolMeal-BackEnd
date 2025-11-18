using SMMS.Application.Features.foodmenu.DTOs;

namespace SMMS.WebAPI.DTOs;
public class SuggestMenuRequest
{
    public Guid UserId { get; set; }      // tạm: FE gửi, sau này lấy từ JWT
    public Guid SchoolId { get; set; }

    public List<int>? MainIngredientIds { get; set; }
    public List<int>? SideIngredientIds { get; set; }
    public List<int>? AvoidAllergenIds { get; set; }

    public double? MaxMainKcal { get; set; } = 600;
    public double? MaxSideKcal { get; set; } = 250;

    public int? TopKMain { get; set; } = 5;
    public int? TopKSide { get; set; } = 5;
}

public class LogAiSelectionRequest
{
    public Guid UserId { get; set; }      // tạm: FE gửi, sau này lấy từ JWT
    public long SessionId { get; set; }

    // dùng lại SelectedDishDto từ Application
    public List<SelectedDishDto> SelectedItems { get; set; } = new();
}
