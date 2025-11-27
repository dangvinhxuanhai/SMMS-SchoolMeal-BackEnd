using Microsoft.AspNetCore.Http;
using SMMS.Application.Features.auth.DTOs;
using SMMS.Application.Features.Skeleton.Interfaces;
using SMMS.Domain.Entities.auth;
using System;
using System.Threading.Tasks;

namespace SMMS.Application.Features.auth.Interfaces
{
    public interface IUserProfileRepository : IRepository<User>
    {
        Task<UserProfileResponseDto> GetUserProfileAsync(Guid userId);
        Task<bool> UpdateUserProfileAsync(Guid userId, UpdateUserProfileDto dto);
        Task<ChildProfileResponseDto> UpdateChildInfoAsync(Guid userId, ChildProfileDto dto);
        Task<string> UploadChildAvatarAsync(IFormFile file, Guid studentId);
        Task<string> UploadUserAvatarAsync(IFormFile file, Guid userId);
    }
}
