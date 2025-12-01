using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SMMS.Application.Features.Menus.DTOs.Schooling;
public sealed class UpdateSchoolDto
{
    public string SchoolName { get; set; } = default!;
    public string? ContactEmail { get; set; }
    public string? Hotline { get; set; }
    public string? SchoolContract { get; set; }
    public string? SchoolAddress { get; set; }
    public bool IsActive { get; set; } = true;
    public Guid? UpdatedBy { get; set; }
}
