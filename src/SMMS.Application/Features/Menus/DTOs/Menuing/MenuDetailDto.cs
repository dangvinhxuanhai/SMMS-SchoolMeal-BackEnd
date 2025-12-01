using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SMMS.Application.Features.Menus.DTOs.Menuing;
public sealed class MenuDetailDto
{
    public int MenuId { get; set; }
    public DateTime? PublishedAt { get; set; }
    public Guid SchoolId { get; set; }
    public bool IsVisible { get; set; }
    public short? WeekNo { get; set; }
    public DateTime CreatedAt { get; set; }
    public Guid? ConfirmedBy { get; set; }
    public DateTime? ConfirmedAt { get; set; }
    public bool AskToDelete { get; set; }
    public int? YearId { get; set; }
}
