using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SMMS.Application.Features.Menus.DTOs.Menuing;
public sealed class MenuListItemDto
{
    public int MenuId { get; set; }
    public Guid SchoolId { get; set; }
    public short? WeekNo { get; set; }
    public bool IsVisible { get; set; }
    public int? YearId { get; set; }
    public DateTime? PublishedAt { get; set; }
}
