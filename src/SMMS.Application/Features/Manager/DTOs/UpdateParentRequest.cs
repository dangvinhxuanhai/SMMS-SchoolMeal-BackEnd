using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SMMS.Application.Features.Manager.DTOs;
public class UpdateParentRequest
{
    public string? FullName { get; set; }
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public string? Password { get; set; }
    public string? Gender { get; set; }
    public string? RelationName { get; set; }
    public Guid? UpdatedBy { get; set; }
    public List<UpdateChildDto>? Children { get; set; }
}

public class UpdateChildDto
{
    public Guid? StudentId { get; set; }
    public string FullName { get; set; } = null!;
    public string? Gender { get; set; }
    public DateTime? DateOfBirth { get; set; }
    public Guid? ClassId { get; set; }
}
