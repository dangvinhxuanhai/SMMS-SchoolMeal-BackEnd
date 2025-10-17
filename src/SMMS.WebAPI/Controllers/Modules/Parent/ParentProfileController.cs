using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SMMS.Application.Features.auth.DTOs;
using SMMS.Application.Features.auth.Interfaces;
using System;
using System.Threading.Tasks;

namespace SMMS.WebAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Parent")]
    public class ParentProfileController : ControllerBase
    {
        private readonly IUserProfileService _userProfileService;

        public ParentProfileController(IUserProfileService userProfileService)
        {
            _userProfileService = userProfileService;
        }

        [HttpGet("{userId}")]
        public async Task<ActionResult<UserProfileResponseDto>> GetUserProfile(Guid userId)
        {
            try
            {
                var profile = await _userProfileService.GetUserProfileAsync(userId);
                return Ok(profile);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPut("{userId}")]
        public async Task<ActionResult<bool>> UpdateUserProfile(Guid userId, [FromBody] UpdateUserProfileDto dto)
        {
            try
            {
                var result = await _userProfileService.UpdateUserProfileAsync(userId, dto);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPost("upload-avatar/{studentId}")]
        public async Task<ActionResult<string>> UploadChildAvatar(Guid studentId, [FromForm] UploadAvatarRequest request)
        {
            try
            {
                var avatarUrl = await _userProfileService.UploadChildAvatarAsync(
                    request.FileName,
                    request.FileData,
                    studentId);
                return Ok(avatarUrl);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }

    public class UploadAvatarRequest
    {
        public string FileName { get; set; }
        public byte[] FileData { get; set; }
    }
}
