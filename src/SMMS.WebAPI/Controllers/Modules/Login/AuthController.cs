using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SMMS.Application.Features.auth.DTOs;
using SMMS.Application.Features.auth.Interfaces;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthRepository _authService;

    public AuthController(IAuthRepository authService)
    {
        _authService = authService;
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequestDto request)
    {
        try
        {
            var result = await _authService.LoginAsync(request);
            if (!string.IsNullOrEmpty(result.Token))
            {
                var cookieOptions = GetCookieOptions(1);
                Response.Cookies.Append("accessToken", result.Token, cookieOptions);
                if (!string.IsNullOrEmpty(result.RefreshToken))
                {
                    Response.Cookies.Append("refreshToken", result.RefreshToken, cookieOptions);
                }
            }

            return Ok(result);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpGet("me")]
    [Authorize]
    public async Task<IActionResult> GetCurrentUser()
    {
        try
        {
            var userIdClaim = User.FindFirst("UserId")?.Value
                              ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                              ?? User.FindFirst("sub")?.Value
                              ?? User.FindFirst("id")?.Value;
            if (string.IsNullOrEmpty(userIdClaim))
                return Unauthorized("Token không hợp lệ.");

            var userId = Guid.Parse(userIdClaim);

            var user = await _authService.GetUserByIdAsync(userId);

            if (user == null)
                return NotFound(new { message = "Không tìm thấy người dùng." });

            return Ok(new UserInfoDto
            {
                UserId = user.UserId,
                FullName = user.FullName,
                Phone = user.Phone,
                Email = user.Email,
                Role = user.Role?.RoleName,
                SchoolId = user.SchoolId
            });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    private CookieOptions GetCookieOptions(int daysExpire)
    {
        return new CookieOptions
        {
            HttpOnly = true,
            Secure = false,
            SameSite = SameSiteMode.None,
            Expires = DateTime.UtcNow.AddDays(daysExpire)
        };
    }

    [HttpPost("refresh-token")]
    public async Task<IActionResult> RefreshToken()
    {
        try
        {
            var refreshToken = Request.Cookies["refreshToken"];

            if (string.IsNullOrEmpty(refreshToken))
            {
                return Unauthorized(new { message = "Không tìm thấy Refresh Token trong Cookie." });
            }

            var result = await _authService.RefreshTokenAsync(refreshToken);

            if (!string.IsNullOrEmpty(result.Token))
            {
                var cookieOptions = GetCookieOptions(7);

                // Lưu Access Token mới
                Response.Cookies.Append("accessToken", result.Token, GetCookieOptions(1));

                if (!string.IsNullOrEmpty(result.RefreshToken))
                {
                    Response.Cookies.Append("refreshToken", result.RefreshToken, cookieOptions);
                }
            }

            return Ok(result);
        }
        catch (Exception ex)
        {
            Response.Cookies.Delete("accessToken");
            Response.Cookies.Delete("refreshToken");
            return Unauthorized(new { message = ex.Message });
        }
    }

    [HttpPost("reset-first-password")]
    public async Task<IActionResult> ResetFirstPassword([FromBody] ResetFirstPasswordRequest request)
    {
        try
        {
            await _authService.ResetFirstPasswordAsync(request.Email, request.CurrentPassword, request.NewPassword);
            return Ok(new { message = "Đổi mật khẩu thành công, vui lòng đăng nhập lại." });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }


    [HttpPost("logout")]
    public async Task<IActionResult> Logout()
    {
        try
        {
            var refreshToken = Request.Cookies["refreshToken"];

            if (!string.IsNullOrEmpty(refreshToken))
            {
                await _authService.LogoutAsync(refreshToken);
            }

            Response.Cookies.Delete("accessToken");
            Response.Cookies.Delete("refreshToken");

            return Ok(new { message = "Đăng xuất thành công" });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
}
