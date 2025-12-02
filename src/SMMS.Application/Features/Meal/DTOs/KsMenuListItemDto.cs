using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SMMS.Application.Features.Meal.DTOs;
public class KsMenuListItemDto
{
    public int MenuId { get; set; }
    public Guid SchoolId { get; set; }
    public short? WeekNo { get; set; }
    public int? YearId { get; set; }

    public DateTime CreatedAt { get; set; }
    public DateTime? PublishedAt { get; set; }

    public bool IsVisible { get; set; }
    public bool AskToDelete { get; set; }
}
