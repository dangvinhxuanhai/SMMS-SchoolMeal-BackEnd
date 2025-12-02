using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SMMS.Application.Features.Menus.DTOs.Schooling;
public sealed class SchoolListItemDto
{
    public Guid SchoolId { get; set; }
    public string SchoolName { get; set; } = default!;
    public string? ContactEmail { get; set; }
    public string? Hotline { get; set; }
    public bool IsActive { get; set; }
}
