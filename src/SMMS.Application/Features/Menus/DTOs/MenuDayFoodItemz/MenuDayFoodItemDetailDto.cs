using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SMMS.Application.Features.Menus.DTOs.MenuDayFoodItemz;
public sealed class MenuDayFoodItemDetailDto
{
    public int MenuDayId { get; set; }
    public int FoodId { get; set; }
    public string? FoodName { get; set; }
    public int? SortOrder { get; set; }
}
