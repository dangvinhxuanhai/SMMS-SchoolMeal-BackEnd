using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SMMS.Application.Features.Manager.DTOs;
public class CreateParentRequest
{
    public string FullName { get; set; } = null!;
    public string? Email { get; set; } 
    public string Phone { get; set; } = null!;
    public string Password { get; set; } = null!;
    public Guid SchoolId { get; set; }
    public string? Gender { get; set; }
    public DateTime? DateOfBirth { get; set; }
    public string? RelationName { get; set; }
    public Guid? CreatedBy { get; set; }
    public List<CreateChildDto> Children { get; set; } = new();
}
public class CreateChildDto
{
    public string FullName { get; set; } = null!;
    public string? Gender { get; set; }
    public DateTime? DateOfBirth { get; set; }
    public Guid ClassId { get; set; }

}
