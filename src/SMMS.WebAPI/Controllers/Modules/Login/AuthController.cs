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

    private void SetRefreshTokenCookie(string refreshToken)
    {
        var cookieOptions = new CookieOptions
        {
            HttpOnly = true, Secure = true, SameSite = SameSiteMode.None, Expires = DateTime.UtcNow.AddDays(7)
        };
        Response.Cookies.Append("refreshToken", refreshToken, cookieOptions);
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequestDto request)
    {
        try
        {
            var result = await _authService.LoginAsync(request);

            Response.Cookies.Append("accessToken", result.Token, new CookieOptions
            {
                HttpOnly = true, Secure = true, SameSite = SameSiteMode.None, Expires = DateTime.UtcNow.AddMinutes(30),
            });

            SetRefreshTokenCookie(result.RefreshToken);

            return Ok(new { user = result.User });
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
            var userIdString = User.FindFirst("UserId")?.Value
                               ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                               ?? User.FindFirst("sub")?.Value
                               ?? User.FindFirst("id")?.Value;

            if (string.IsNullOrEmpty(userIdString))
                return Unauthorized(new { message = "Token không chứa User ID." });

            if (!Guid.TryParse(userIdString, out Guid userId))
            {
                return Unauthorized(new { message = $"User ID trong token không hợp lệ: {userIdString}" });
            }

            var user = await _authService.GetUserByIdAsync(userId);

            if (user == null)
                return NotFound(new { message = "Không tìm thấy thông tin người dùng." });

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
            return BadRequest(new { message = $"Lỗi Server: {ex.Message}" });
        }
    }

    [HttpPost("refresh-token")]
    public async Task<IActionResult> RefreshToken()
    {
        try
        {
            var refreshToken = Request.Cookies["refreshToken"];

            if (string.IsNullOrEmpty(refreshToken))
            {
                var cookieOptions = new CookieOptions
                {
                    HttpOnly = true,
                    Secure = true,
                    SameSite = SameSiteMode.None,
                    Expires = DateTime.UtcNow.AddDays(-1)
                };
                Response.Cookies.Delete("accessToken", cookieOptions);
                Response.Cookies.Delete("refreshToken", cookieOptions);

                return Unauthorized(new { message = "Phiên đăng nhập đã hết hạn" });
            }

            var result = await _authService.RefreshTokenAsync(refreshToken);

            if (string.IsNullOrEmpty(result.Token))
            {
                throw new Exception("Lỗi tạo Access Token mới.");
            }

            Response.Cookies.Append("accessToken", result.Token, new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.None,
                Expires = DateTime.UtcNow.AddMinutes(30),
            });

            if (!string.IsNullOrEmpty(result.RefreshToken))
            {
                SetRefreshTokenCookie(result.RefreshToken);
            }

            return Ok(new { message = "Refresh token thành công" });
        }
        catch (Exception ex)
        {
            var cookieOptions = new CookieOptions
            {
                HttpOnly = true, Secure = true, SameSite = SameSiteMode.None, Expires = DateTime.UtcNow.AddDays(-1)
            };
            Response.Cookies.Delete("refreshToken", cookieOptions);
            Response.Cookies.Delete("accessToken", cookieOptions);

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

            // Xóa token trong DB nếu cần thiết
            if (!string.IsNullOrEmpty(refreshToken))
            {
                await _authService.LogoutAsync(refreshToken);
            }

            var cookieOptions = new CookieOptions
            {
                HttpOnly = true, Secure = true, SameSite = SameSiteMode.None, Expires = DateTime.UtcNow.AddDays(-1)
            };

            Response.Cookies.Delete("refreshToken", cookieOptions);
            Response.Cookies.Delete("accessToken", cookieOptions);

            return Ok(new { message = "Đăng xuất thành công" });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
}
