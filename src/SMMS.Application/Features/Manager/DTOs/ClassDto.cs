using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SMMS.Application.Features.Manager.DTOs;
public class ClassDto
{
    public Guid ClassId { get; set; }
    public string ClassName { get; set; } = null!;
    public Guid SchoolId { get; set; }
    public int YearId { get; set; }
    public Guid? TeacherId { get; set; }
    public string? TeacherName { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class CreateClassRequest
{
    public string ClassName { get; set; } = null!;
    public Guid SchoolId { get; set; }
    public int YearId { get; set; }
    public Guid? TeacherId { get; set; }
    public Guid? CreatedBy { get; set; }
}

public class UpdateClassRequest
{
    public string? ClassName { get; set; }
    public Guid? TeacherId { get; set; }
    public bool? IsActive { get; set; }
    public Guid? UpdatedBy { get; set; }
}
