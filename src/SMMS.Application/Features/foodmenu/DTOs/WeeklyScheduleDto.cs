using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SMMS.Application.Features.foodmenu.DTOs;
public class WeeklyScheduleDto
{
    public long ScheduleMealId { get; set; }
    public Guid SchoolId { get; set; }

    public DateTime WeekStart { get; set; }
    public DateTime WeekEnd { get; set; }
    public short WeekNo { get; set; }
    public short YearNo { get; set; }
    public string Status { get; set; } = default!;
    public string? Notes { get; set; }

    public List<DailyMealDto> DailyMeals { get; set; } = new();
}
