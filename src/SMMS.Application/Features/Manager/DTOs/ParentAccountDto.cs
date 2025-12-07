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
    public string RelationName { get; set; }
    public bool IsActive { get; set; }
    public bool IsDefaultPassword { get; set; }
    public string PaymentStatus { get; set; } = "Chưa tạo hóa đơn";
    public DateTime CreatedAt { get; set; }
    public List<ParentStudentDetailDto> Children { get; set; }

    public class ParentStudentDetailDto
    {
        public Guid? StudentId { get; set; }
        public string FullName { get; set; }
        public string Gender { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public Guid? ClassId { get; set; }
        public string ClassName { get; set; }
    }
}
