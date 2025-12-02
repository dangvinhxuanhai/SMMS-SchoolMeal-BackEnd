using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SMMS.Application.Features.Menus.DTOs.MenuDaying;
public sealed class UpdateMenuDayDto
{
    public byte DayOfWeek { get; set; }    // 1..7
    public string MealType { get; set; } = default!;
    public string? Notes { get; set; }
}
