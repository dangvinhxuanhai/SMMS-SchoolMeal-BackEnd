using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SMMS.Application.Features.foodmenu.DTOs;
public record WeekOptionDto(
    long ScheduleMealId,
    short WeekNo,
    short YearNo,
    DateTime WeekStart,
    DateTime WeekEnd,
    string Status
);
