using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using SMMS.Application.Features.auth.DTOs;
using SMMS.Application.Features.auth.Interfaces;
using SMMS.Domain.Entities.auth;
using SMMS.Infrastructure.Security;
using SMMS.Persistence.Data;
using System;
using System.Threading.Tasks;
using SMMS.Application.Common.Interfaces;

namespace SMMS.Infrastructure.Service
{
    public class AuthRepository : IAuthRepository
    {
        private readonly EduMealContext _dbContext;
        private readonly IJwtService _jwtService;
        private readonly IConfiguration _configuration;
        private readonly IPasswordHasher _passwordHasher;

        public AuthRepository(EduMealContext dbContext, IJwtService jwtService, IConfiguration configuration,
            IPasswordHasher passwordHasher)
        {
            _dbContext = dbContext;
            _jwtService = jwtService;
            _configuration = configuration;
            _passwordHasher = passwordHasher;
        }

        // ✅ Đăng nhập bằng SĐT hoặc Email
        public async Task<LoginResponseDto> LoginAsync(LoginRequestDto request)
        {
            // Tìm người dùng theo số điện thoại hoặc email
            var user = await _dbContext.Users
                .Include(u => u.Role)
                .FirstOrDefaultAsync(u =>
                    u.Phone == request.PhoneOrEmail || u.Email == request.PhoneOrEmail);

            if (user == null)
                throw new Exception("Tài khoản không tồn tại.");
            // Kiểm tra tài khoản có bị Ban không
            if (!user.IsActive)
                throw new Exception("Tài khoản của bạn đã bị khóa. Vui lòng liên hệ quản trị viên.");
            // Kiểm tra mật khẩu
            if (!_passwordHasher.VerifyPassword(request.Password, user.PasswordHash))
                throw new Exception("Mật khẩu không đúng.");

            // ✅ Kiểm tra xem có đang dùng mật khẩu tạm không
            bool isUsingTempPassword = _passwordHasher.VerifyPassword("@1", user.PasswordHash);

            if (isUsingTempPassword)
            {
                return new LoginResponseDto
                {
                    RequirePasswordReset = true,
                    Message = "Tài khoản đang sử dụng mật khẩu tạm, vui lòng đổi mật khẩu để kích hoạt."
                };
            }

            // ✅ Sinh JWT token
            string token = _jwtService.GenerateToken(user, user.Role.RoleName);

            string refreshToken = _jwtService.GenerateRefreshToken();
            var refreshTokenEntity = new RefreshToken
            {
                UserId = user.UserId,
                Token = refreshToken,
                ExpiresAt = DateTime.UtcNow.AddDays(7),
                CreatedAt = DateTime.UtcNow,
                CreatedByIp = "UserLogin",
                RevokedAt = null
            };
            await _dbContext.RefreshTokens.AddAsync(refreshTokenEntity);
            await _dbContext.SaveChangesAsync();

            return new LoginResponseDto
            {
                Token = token,
                RefreshToken = refreshToken,
                Message = "Đăng nhập thành công.",
                User = new UserInfoDto
                {
                    UserId = user.UserId,
                    FullName = user.FullName,
                    Phone = user.Phone,
                    Email = user.Email,
                    Role = user.Role.RoleName,
                    SchoolId = user.SchoolId
                }
            };
        }

        // ✅ Refresh token
        public async Task<LoginResponseDto> RefreshTokenAsync(string refreshToken)
        {
            var storedRefreshToken = await _dbContext.RefreshTokens
                .Include(rt => rt.User)
                .ThenInclude(u => u.Role)
                .FirstOrDefaultAsync(rt =>
                    rt.Token == refreshToken &&
                    rt.ExpiresAt > DateTime.UtcNow &&
                    rt.RevokedAt == null);

            if (storedRefreshToken == null)
                throw new Exception("Refresh token không hợp lệ hoặc đã hết hạn."); // vẫn throw, nhưng chỉ khi CÓ cookie mà sai
            var newToken = _jwtService.GenerateToken(storedRefreshToken.User, storedRefreshToken.User.Role.RoleName);
            var newRefreshTokenString = _jwtService.GenerateRefreshToken();

            var refreshTokenEntity = new RefreshToken
            {
                UserId = storedRefreshToken.UserId,
                Token = newRefreshTokenString,
                ExpiresAt = DateTime.UtcNow.AddDays(7),
                CreatedAt = DateTime.UtcNow,
                CreatedByIp = "System",
                RevokedAt = null,
                ReplacedById = null
            };
            await _dbContext.RefreshTokens.AddAsync(refreshTokenEntity);
            await _dbContext.SaveChangesAsync();

            storedRefreshToken.RevokedAt = DateTime.UtcNow;
            storedRefreshToken.ReplacedById = refreshTokenEntity.RefreshTokenId;

            _dbContext.RefreshTokens.Update(storedRefreshToken);
            await _dbContext.SaveChangesAsync();

            return new LoginResponseDto
            {
                Token = newToken,
                RefreshToken = newRefreshTokenString,
                Expiration = DateTime.UtcNow.AddMinutes(Convert.ToDouble(_configuration["Jwt:ExpireMinutes"])),
                User = new UserInfoDto
                {
                    UserId = storedRefreshToken.User.UserId,
                    FullName = storedRefreshToken.User.FullName,
                    Phone = storedRefreshToken.User.Phone,
                    Email = storedRefreshToken.User.Email,
                    Role = storedRefreshToken.User.Role.RoleName,
                    SchoolId = storedRefreshToken.User.SchoolId
                }
            };
        }

        public async Task<User?> GetUserByIdAsync(Guid userId)
        {
            return await _dbContext.Users
                .Include(u => u.Role)
                .Include(u => u.School)
                .FirstOrDefaultAsync(u => u.UserId == userId);
        }

        // ✅ Đăng xuất
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

        // ✅ Reset mật khẩu lần đầu (khi đang dùng mật khẩu tạm)
        public async Task ResetFirstPasswordAsync(string phoneOrEmail, string currentPassword, string newPassword)
        {
            var user = await _dbContext.Users
                .FirstOrDefaultAsync(u => u.Phone == phoneOrEmail || u.Email == phoneOrEmail);

            if (user == null)
                throw new Exception("Không tìm thấy tài khoản.");

            if (!_passwordHasher.VerifyPassword(currentPassword, user.PasswordHash))
                throw new Exception("Mật khẩu hiện tại không đúng.");

            bool isTemp = _passwordHasher.VerifyPassword("@1", user.PasswordHash);
            if (!isTemp)
                throw new Exception("Tài khoản đã được đổi mật khẩu trước đó.");

            user.PasswordHash = _passwordHasher.HashPassword(newPassword);
            await _dbContext.SaveChangesAsync();
        }
    }
}
