using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SMMS.Application.Features.Menus.DTOs.Menuing;
public sealed class CreateMenuDto
{
    public DateTime? PublishedAt { get; set; }
    public Guid SchoolId { get; set; }
    public bool IsVisible { get; set; } = true;
    public short? WeekNo { get; set; }
    public Guid? ConfirmedBy { get; set; }
    public DateTime? ConfirmedAt { get; set; }
    public bool AskToDelete { get; set; } = false;
    public int? YearId { get; set; }
}
