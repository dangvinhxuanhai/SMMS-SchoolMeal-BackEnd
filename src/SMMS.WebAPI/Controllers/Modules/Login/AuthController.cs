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
                var cookieOptions = new CookieOptions
                {
                    HttpOnly = true,
                    Secure = true,
                    SameSite = SameSiteMode.Strict,
                    Expires = DateTime.Now.AddDays(1)
                };
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
                var cookieOptions = new CookieOptions
                {
                    HttpOnly = true,
                    Secure = true,
                    SameSite = SameSiteMode.Strict,
                    Expires = DateTime.UtcNow.AddDays(7)
                };

                // Lưu Access Token mới
                Response.Cookies.Append("accessToken", result.Token, new CookieOptions
                {
                    HttpOnly = true,
                    Secure = true,
                    SameSite = SameSiteMode.Strict,
                    Expires = DateTime.UtcNow.AddDays(1)
                });

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
