using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SMMS.Application.Features.Menus.DTOs.MenuDaying;
public sealed class MenuDayDetailDto
{
    public int MenuDayId { get; set; }
    public int MenuId { get; set; }
    public byte DayOfWeek { get; set; }
    public string MealType { get; set; } = default!;
    public string? Notes { get; set; }
}
