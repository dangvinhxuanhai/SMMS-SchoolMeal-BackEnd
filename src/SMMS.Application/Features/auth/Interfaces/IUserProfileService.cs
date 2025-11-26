using SMMS.Application.Features.auth.DTOs;
using SMMS.Application.Features.Skeleton.Interfaces;
using SMMS.Domain.Entities.auth;
using System;
using System.Threading.Tasks;

namespace SMMS.Application.Features.auth.Interfaces
{
    public interface IUserProfileService : IService<User>
    {
        Task<UserProfileResponseDto> GetUserProfileAsync(Guid userId);
        Task<bool> UpdateUserProfileAsync(Guid userId, UpdateUserProfileDto dto);
        Task<string> UploadChildAvatarAsync(string fileName, byte[] fileData, Guid studentId);
    }
}