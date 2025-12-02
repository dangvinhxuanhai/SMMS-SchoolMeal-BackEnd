using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SMMS.Application.Features.school.DTOs
{
    public class StudentImageDto
    {
        public Guid ImageId { get; set; }
        public string ImageUrl { get; set; } = string.Empty; // Cloudinary URL
        public string? Caption { get; set; }
        public DateTime? TakenAt { get; set; }
        public DateTime CreatedAt { get; set; }
    }

}
