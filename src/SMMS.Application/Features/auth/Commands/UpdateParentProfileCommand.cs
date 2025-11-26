using System;
using MediatR;
using Microsoft.AspNetCore.Http;
using SMMS.Application.Features.auth.DTOs;

namespace SMMS.Application.Features.auth.Commands
{
    // FIX: Cập nhật Parent thì trả về bool (hoặc UserProfileResponseDto), KHÔNG trả về ChildProfileResponseDto
    public class UpdateParentProfileCommand : IRequest<bool>
    {
        public Guid ParentId { get; set; }
        public UpdateUserProfileDto Dto { get; set; }

        public UpdateParentProfileCommand(Guid parentId, UpdateUserProfileDto dto)
        {
            ParentId = parentId;
            Dto = dto;
        }
    }

    public class UploadChildAvatarCommand : IRequest<string>
    {
        public Guid ParentId { get; set; }
        public Guid StudentId { get; set; }
        public IFormFile File { get; set; }

        public UploadChildAvatarCommand(Guid parentId, Guid studentId, IFormFile file)
        {
            ParentId = parentId;
            StudentId = studentId;
            File = file;
        }
    }

    public class UploadParentAvatarCommand : IRequest<string>
    {
        public Guid ParentId { get; set; }
        public IFormFile File { get; set; }

        public UploadParentAvatarCommand(Guid parentId, IFormFile file)
        {
            ParentId = parentId;
            File = file;
        }
    }

    // FIX: Thêm dấu ? để cho phép null (nếu không tìm thấy student)
    public class UpdateChildProfileCommand : IRequest<ChildProfileResponseDto?>
    {
        public Guid ParentId { get; }
        public ChildProfileDto Dto { get; }

        public UpdateChildProfileCommand(Guid parentId, ChildProfileDto dto)
        {
            ParentId = parentId;
            Dto = dto;
        }
    }
}
