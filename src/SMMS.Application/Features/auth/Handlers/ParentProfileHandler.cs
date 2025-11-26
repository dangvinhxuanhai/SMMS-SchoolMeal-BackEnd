using System.Threading;
using System.Threading.Tasks;
using MediatR;
using SMMS.Application.Features.auth.Commands;
using SMMS.Application.Features.auth.DTOs;
using SMMS.Application.Features.auth.Interfaces;
using SMMS.Application.Features.auth.Queries;

namespace SMMS.Application.Features.auth.Handlers
{
    public class ParentProfileHandler :
        IRequestHandler<GetParentProfileQuery, UserProfileResponseDto>,
        IRequestHandler<UpdateParentProfileCommand, bool>, // FIX: Sửa lại thành bool
        IRequestHandler<UploadChildAvatarCommand, string>,
        IRequestHandler<UploadParentAvatarCommand, string>
    {
        private readonly IUserProfileRepository _userProfileRepository;

        public ParentProfileHandler(IUserProfileRepository userProfileRepository)
        {
            _userProfileRepository = userProfileRepository;
        }

        public async Task<UserProfileResponseDto> Handle(GetParentProfileQuery request, CancellationToken cancellationToken)
        {
            return await _userProfileRepository.GetUserProfileAsync(request.ParentId);
        }

        // FIX: Sửa return type thành bool
        public async Task<bool> Handle(UpdateParentProfileCommand request, CancellationToken cancellationToken)
        {
            return await _userProfileRepository.UpdateUserProfileAsync(request.ParentId, request.Dto);
        }

        public async Task<string> Handle(UploadChildAvatarCommand request, CancellationToken cancellationToken)
        {
            return await _userProfileRepository.UploadChildAvatarAsync(request.File, request.StudentId);
        }

        public async Task<string> Handle(UploadParentAvatarCommand request, CancellationToken cancellationToken)
        {
            return await _userProfileRepository.UploadUserAvatarAsync(request.File, request.ParentId);
        }
    }

    // FIX: Handler cho Child trả về ChildProfileResponseDto?
    public class ChildProfileHandler :
        IRequestHandler<UpdateChildProfileCommand, ChildProfileResponseDto?>
    {
        private readonly IUserProfileRepository _userProfileRepository;

        public ChildProfileHandler(IUserProfileRepository userProfileRepository)
        {
            _userProfileRepository = userProfileRepository;
        }

        public async Task<ChildProfileResponseDto?> Handle(UpdateChildProfileCommand request, CancellationToken cancellationToken)
        {
            return await _userProfileRepository.UpdateChildInfoAsync(
                request.ParentId,
                request.Dto
            );
        }
    }
}
