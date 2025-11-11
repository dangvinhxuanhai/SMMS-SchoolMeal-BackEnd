using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SMMS.Application.Features.Manager.DTOs;
public class ParentAccountDto
{
    public Guid UserId { get; set; }
    public string FullName { get; set; } = null!;
    public string? Email { get; set; }
    public string Phone { get; set; } = null!;
    public string Role { get; set; } = null!;
    public string? SchoolName { get; set; }
    public string? ClassName { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public List<string>? ChildrenNames { get; set; }
}
