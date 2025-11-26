using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace SMMS.Application.Features.school.DTOs
{
    public class SchoolDTO
    {
        public Guid SchoolId { get; set; }
        public string SchoolName { get; set; } = string.Empty;
        public string? ContactEmail { get; set; }
        public string? Hotline { get; set; }
        public string? SchoolAddress { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public int StudentCount { get; set; }
    }
    public class CreateSchoolDto
    {
        public string SchoolName { get; set; } = null!;
        public string? ContactEmail { get; set; }
        public string? Hotline { get; set; }
        public string? SchoolAddress { get; set; }
        public bool IsActive { get; set; }
    }

    public class UpdateSchoolDto
    {
        public string SchoolName { get; set; } = null!;
        public string? ContactEmail { get; set; }
        public string? Hotline { get; set; }
        public string? SchoolAddress { get; set; }
        public bool IsActive { get; set; }
    }
}
