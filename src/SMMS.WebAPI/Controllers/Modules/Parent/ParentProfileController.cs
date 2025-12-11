using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SMMS.Application.Features.auth.Commands;
using SMMS.Application.Features.auth.DTOs;
using SMMS.Application.Features.auth.Interfaces;
using SMMS.Application.Features.auth.Queries;
using System;
using System.Security.Claims;
using System.Threading.Tasks;

namespace SMMS.WebAPI.Controllers.Modules.Parent
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Parent")]
    public class ParentProfileController : ControllerBase
    {
        private readonly IMediator _mediator;

        public ParentProfileController(IMediator mediator)
        {
            _mediator = mediator;
        }

        // GET: Lấy profile
        [HttpGet("profile")]
        public async Task<ActionResult<UserProfileResponseDto>> GetUserProfile()
        {
            var parentId = GetCurrentUserId();
            var query = new GetParentProfileQuery(parentId);
            var profile = await _mediator.Send(query);
            return Ok(profile);
        }

        // PUT: Cập nhật profile (có upload avatar)
        [HttpPut("profile")]
        [Consumes("multipart/form-data")] // quan trọng để Swagger hiểu
        public async Task<ActionResult<bool>> UpdateUserProfile([FromForm] UpdateUserProfileDto dto)
        {
            var parentId = GetCurrentUserId();
            var command = new UpdateParentProfileCommand(parentId, dto);
            var result = await _mediator.Send(command);
            return Ok(result);
        }
        // PUT: Cập nhật children profile
        [HttpPut("child")]
        [Consumes("multipart/form-data")]
        public async Task<ActionResult<bool>> UpdateChildProfile([FromForm] ChildProfileDto  dto)
        {
            var parentId = GetCurrentUserId();
            var command = new UpdateChildProfileCommand(parentId, dto);
            var result = await _mediator.Send(command);
            return Ok(result);
        }
        // POST: Upload avatar parent
        [HttpPost("upload-avatar")]
        [Consumes("multipart/form-data")]
        public async Task<ActionResult<string>> UploadParentAvatar([FromForm] UploadAvatarRequest request)
        {
            var parentId = GetCurrentUserId();
            var command = new UploadParentAvatarCommand(parentId, request.File);
            var avatarUrl = await _mediator.Send(command);
            return Ok(new { avatarUrl });
        }

        // POST: Upload avatar con
        [HttpPost("upload-avatar/{studentId:guid}")]
        [Consumes("multipart/form-data")]
        public async Task<ActionResult<string>> UploadChildAvatar(Guid studentId, [FromForm] UploadAvatarRequest request)
        {
            var parentId = GetCurrentUserId();
            var command = new UploadChildAvatarCommand(parentId, studentId, request.File);
            var avatarUrl = await _mediator.Send(command);
            return Ok(new { avatarUrl });
        }

        private Guid GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null)
                throw new UnauthorizedAccessException("Không tìm thấy ID người dùng trong token.");
            return Guid.Parse(userIdClaim.Value);
        }
    }

    public class UploadAvatarRequest
    {
        public IFormFile File { get; set; }
    }


}
