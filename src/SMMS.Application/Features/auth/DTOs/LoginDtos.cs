using System;

namespace SMMS.Application.Features.auth.DTOs
{
    public class LoginRequestDto
    {
        public string PhoneOrEmail { get; set; }
        public string Password { get; set; }
    }

    public class LoginResponseDto
    {
        // Nếu user được cấp token bình thường
        public string Token { get; set; }
        public string RefreshToken { get; set; }
        public DateTime? Expiration { get; set; }

        // Dùng khi yêu cầu đổi mật khẩu lần đầu
        public bool RequirePasswordReset { get; set; } = false;

        // Thông điệp trả về cho client (ví dụ: "Vui lòng đổi mật khẩu lần đầu")
        public string Message { get; set; }

        public UserInfoDto User { get; set; }
    }

    public class UserInfoDto
    {
        public Guid UserId { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public string Role { get; set; }
        public Guid? SchoolId { get; set; }
    }

    public class RefreshTokenRequestDto
    {
        public string RefreshToken { get; set; }
    }

    public class ResetPasswordRequest
    {
        public string Email { get; set; }
        public string NewPassword { get; set; }
    }

    public class ResetFirstPasswordRequest
    {
        public string Email { get; set; }
        public string CurrentPassword { get; set; }
        public string NewPassword { get; set; }
    }

    // Lưu ý: lớp LoginResponse (không có "Dto") nếu không dùng bạn có thể xóa để tránh trùng ý nghĩa.
    public class LoginResponse
    {
        public string Token { get; set; }
        public string Message { get; set; }
        public bool RequirePasswordReset { get; set; }
    }
}
