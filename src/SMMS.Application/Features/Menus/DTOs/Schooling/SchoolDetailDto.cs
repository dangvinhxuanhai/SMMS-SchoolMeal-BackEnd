using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SMMS.Application.Features.Menus.DTOs.Schooling;
public sealed class SchoolDetailDto
{
    public Guid SchoolId { get; set; }
    public string SchoolName { get; set; } = default!;
    public string? ContactEmail { get; set; }
    public string? Hotline { get; set; }
    public string? SchoolContract { get; set; }
    public string? SchoolAddress { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public Guid? CreatedBy { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public Guid? UpdatedBy { get; set; }
}
