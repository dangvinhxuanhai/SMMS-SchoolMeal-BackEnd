using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SMMS.Application.Features.auth.DTOs;
using SMMS.Application.Features.auth.Interfaces;
using System;
using System.Security.Claims;
using System.Threading.Tasks;

namespace SMMS.WebAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Parent")] // chỉ cho phép role Parent
    public class ParentProfileController : ControllerBase
    {
        private readonly IUserProfileService _userProfileService;

        public ParentProfileController(IUserProfileService userProfileService)
        {
            _userProfileService = userProfileService;
        }

        // ✅ Lấy thông tin hồ sơ của chính phụ huynh đang đăng nhập
        [HttpGet("profile")]
        public async Task<ActionResult<UserProfileResponseDto>> GetUserProfile()
        {
            try
            {
                var userId = GetCurrentUserId(); // lấy ID từ token
                var profile = await _userProfileService.GetUserProfileAsync(userId);

                if (profile == null)
                    return NotFound(new { message = "Không tìm thấy thông tin hồ sơ người dùng." });

                return Ok(profile);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Lỗi hệ thống: {ex.Message}" });
            }
        }

        // ✅ Cập nhật hồ sơ cá nhân của chính phụ huynh
        [HttpPut("profile")]
        public async Task<ActionResult<bool>> UpdateUserProfile([FromBody] UpdateUserProfileDto dto)
        {
            try
            {
                var userId = GetCurrentUserId(); // lấy ID từ token
                var result = await _userProfileService.UpdateUserProfileAsync(userId, dto);
                return Ok(result);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Lỗi hệ thống: {ex.Message}" });
            }
        }

        // ✅ Upload avatar cho con của phụ huynh đang đăng nhập
        [HttpPost("upload-avatar/{studentId:guid}")]
        public async Task<ActionResult<string>> UploadChildAvatar(Guid studentId, [FromForm] UploadAvatarRequest request)
        {
            try
            {
                var parentId = GetCurrentUserId(); // có thể kiểm tra quyền sở hữu học sinh
                var avatarUrl = await _userProfileService.UploadChildAvatarAsync(
                    request.FileName,
                    request.FileData,
                    studentId);

                return Ok(new { avatarUrl });
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Lỗi hệ thống: {ex.Message}" });
            }
        }

        // 🔹 Hàm tiện ích lấy userId từ JWT token
        private Guid GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null)
            {
                throw new UnauthorizedAccessException("Không tìm thấy ID người dùng trong token.");
            }

            return Guid.Parse(userIdClaim.Value);
        }
    }

    public class UploadAvatarRequest
    {
        public string FileName { get; set; }
        public byte[] FileData { get; set; }
    }
}
