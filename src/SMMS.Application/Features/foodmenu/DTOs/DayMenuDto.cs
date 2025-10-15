using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SMMS.Application.Features.foodmenu.DTOs;
public record DayMenuDto(
    DateTime MealDate,
    string MealType,                    // Breakfast/Lunch/Snack...
    string? Notes,
    IReadOnlyList<MenuFoodItemDto> Items
);
