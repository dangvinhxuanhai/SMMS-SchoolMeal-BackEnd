using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using SMMS.Application.Features.auth.DTOs;
using SMMS.Application.Features.auth.Interfaces;
using SMMS.Domain.Models.auth;
using SMMS.Persistence.DbContextSite;
using SMMS.Infrastructure.Security; // ✅ thêm để dùng PasswordHasher
using System;
using System.Threading.Tasks;

namespace SMMS.Infrastructure.Service
{
    public class AuthService : IAuthService
    {
        private readonly EduMealContext _dbContext;
        private readonly IJwtService _jwtService;
        private readonly IConfiguration _configuration;

        public AuthService(EduMealContext dbContext, IJwtService jwtService, IConfiguration configuration)
        {
            _dbContext = dbContext;
            _jwtService = jwtService;
            _configuration = configuration;
        }

        public async Task<LoginResponseDto> LoginAsync(LoginRequestDto request)
        {
            var user = await _dbContext.Users
                .Include(u => u.Role)
                .FirstOrDefaultAsync(u => u.Email == request.Email);

            if (user == null)
                throw new Exception("Email không tồn tại.");

            if (!PasswordHasher.VerifyPassword(request.Password, user.PasswordHash))
                throw new Exception("Mật khẩu không đúng.");

            // Kiểm tra nếu mật khẩu hiện tại là mật khẩu tạm (@1)
            bool isUsingTempPassword = PasswordHasher.VerifyPassword("@1", user.PasswordHash);

            if (isUsingTempPassword)
            {
                return new LoginResponseDto
                {
                    RequirePasswordReset = true,
                    Message = "Tài khoản đang sử dụng mật khẩu tạm, vui lòng đổi mật khẩu để kích hoạt."
                };
            }

            // TODO: sinh JWT token ở đây như bình thường
            string token = _jwtService.GenerateToken(user, user.Role.RoleName);

            return new LoginResponseDto
            {
                Token = token,
                Message = "Đăng nhập thành công."
            };
        }


        public async Task<LoginResponseDto> RefreshTokenAsync(string refreshToken)
        {
            var storedRefreshToken = await _dbContext.RefreshTokens
                .Include(rt => rt.User)
                .ThenInclude(u => u.Role)
                .FirstOrDefaultAsync(rt => rt.Token == refreshToken && rt.ExpiresAt > DateTime.UtcNow && rt.RevokedAt == null);

            if (storedRefreshToken == null)
                throw new Exception("Refresh token không hợp lệ.");

            var newToken = _jwtService.GenerateToken(storedRefreshToken.User, storedRefreshToken.User.Role.RoleName);
            var newRefreshToken = _jwtService.GenerateRefreshToken();

            storedRefreshToken.RevokedAt = DateTime.UtcNow;
            storedRefreshToken.ReplacedById = (await _dbContext.RefreshTokens.AddAsync(new RefreshToken
            {
                UserId = storedRefreshToken.UserId,
                Token = newRefreshToken,
                ExpiresAt = DateTime.UtcNow.AddDays(7),
                CreatedAt = DateTime.UtcNow,
                CreatedByIp = "System"
            })).Entity.RefreshTokenId;

            await _dbContext.SaveChangesAsync();

            return new LoginResponseDto
            {
                Token = newToken,
                RefreshToken = newRefreshToken,
                Expiration = DateTime.UtcNow.AddMinutes(Convert.ToDouble(_configuration["Jwt:ExpireMinutes"])),
                User = new UserInfoDto
                {
                    UserId = storedRefreshToken.User.UserId,
                    FullName = storedRefreshToken.User.FullName,
                    Email = storedRefreshToken.User.Email,
                    Phone = storedRefreshToken.User.Phone,
                    Role = storedRefreshToken.User.Role.RoleName,
                    SchoolId = storedRefreshToken.User.SchoolId
                }
            };
        }

        public async Task<bool> LogoutAsync(string refreshToken)
        {
            var storedRefreshToken = await _dbContext.RefreshTokens
                .FirstOrDefaultAsync(rt => rt.Token == refreshToken && rt.RevokedAt == null);

            if (storedRefreshToken != null)
            {
                storedRefreshToken.RevokedAt = DateTime.UtcNow;
                await _dbContext.SaveChangesAsync();
            }

            return true;
        }

        private async Task LogLoginAttempt(Guid? userId, string username, bool succeeded)
        {
            var attempt = new LoginAttempt
            {
                UserId = userId,
                UserName = username,
                AttemptAt = DateTime.UtcNow,
                IpAddress = "System",
                Succeeded = succeeded
            };

            _dbContext.LoginAttempts.Add(attempt);
            await _dbContext.SaveChangesAsync();
        }
        public async Task ResetFirstPasswordAsync(string email, string currentPassword, string newPassword)
        {
            var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.Email == email);
            if (user == null)
                throw new Exception("Không tìm thấy tài khoản.");

            // Kiểm tra mật khẩu hiện tại có đúng không
            if (!PasswordHasher.VerifyPassword(currentPassword, user.PasswordHash))
                throw new Exception("Mật khẩu hiện tại không đúng.");

            // Kiểm tra có đang dùng mật khẩu tạm không
            bool isTemp = PasswordHasher.VerifyPassword("@1", user.PasswordHash);
            if (!isTemp)
                throw new Exception("Tài khoản đã được đổi mật khẩu trước đó.");

            // Cập nhật mật khẩu mới (hash lại)
            user.PasswordHash = PasswordHasher.HashPassword(newPassword);
            await _dbContext.SaveChangesAsync();
        }

    }
}
