using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SMMS.Application.Features.foodmenu.DTOs;
public class DailyMealDto
{
    public int DailyMealId { get; set; }
    public DateTime MealDate { get; set; }
    public string MealType { get; set; } = default!;  // Sáng/Trưa/Chiều...
    public long ScheduleMealId { get; set; }
    public string? Notes { get; set; }
}
