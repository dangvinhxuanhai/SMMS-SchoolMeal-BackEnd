using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Http;

namespace SMMS.Application.Features.auth.DTOs
{
    public class UserProfileResponseDto
    {
        public string FullName { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public string AvatarUrl { get; set; }
        public string? DateOfBirth { get; set; }

        public List<ChildProfileResponseDto> Children { get; set; } = new();
    }

    public class ChildProfileResponseDto
    {
        public Guid StudentId { get; set; }
        public string FullName { get; set; }
        public string AvatarUrl { get; set; }
        public string Relation { get; set; }
        public List<string> AllergyFoods { get; set; } = new();
        public string ClassName { get; set; }
        public DateOnly? DateOfBirth { get; set; }
        public string? Gender { get; set; }
    }

    public class UpdateUserProfileDto
    {
        public string FullName { get; set; }
        public string? AvatarUrl { get; set; }     // Hứng link ảnh (nếu không upload file mới)
        public string Email { get; set; }
        public string Phone { get; set; }
        public IFormFile? AvatarFile { get; set; }
        public DateOnly? DateOfBirth { get; set; }
        public string? ChildrenJson { get; set; }
    }

    public class ChildProfileDto
    {
        public Guid StudentId { get; set; }
        public string? FullName { get; set; }
        public IFormFile? AvatarFile { get; set; }
        public string Relation { get; set; }
        public string? AvatarUrl { get; set; }     // Hứng link ảnh (nếu không upload file mới)
        public DateOnly? DateOfBirth { get; set; }
        public List<string>? AllergyFoods { get; set; } = new();
        public string? Gender { get; set; }
    }
}
