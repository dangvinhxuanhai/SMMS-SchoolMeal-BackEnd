using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SMMS.Application.Features.Meal.DTOs;
public class OffDateCheckResultDto
{
    public bool HasOffDates { get; set; }
    public List<OffDateItemDto> OffDates { get; set; } = new();
}

public class OffDateItemDto
{
    public DateOnly Date { get; set; }
    public string DayOfWeek { get; set; } = null!; // T2, T3, ...
}

