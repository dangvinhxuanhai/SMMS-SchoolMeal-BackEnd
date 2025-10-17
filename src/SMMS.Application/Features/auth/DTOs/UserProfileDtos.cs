using System;
using System.Collections.Generic;

namespace SMMS.Application.Features.auth.DTOs
{
    public class UserProfileResponseDto
    {
        public string FullName { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public List<ChildProfileResponseDto> Children { get; set; } = new();
    }

    public class ChildProfileResponseDto
    {
        public Guid StudentId { get; set; }
        public string FullName { get; set; }
        public string AvatarUrl { get; set; }
        public List<string> AllergyFoods { get; set; } = new();
        public string ClassName { get; set; }
    }

    public class UpdateUserProfileDto
    {
        public string FullName { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public List<ChildProfileDto> Children { get; set; } = new();
    }

    public class ChildProfileDto
    {
        public Guid StudentId { get; set; }
        public string? FullName { get; set; }
        public string? AvatarFileName { get; set; }
        public byte[]? AvatarFileData { get; set; }
        public List<string>? AllergyFoods { get; set; } = new();
        public string? FoodPreferences { get; set; }
    }
}
