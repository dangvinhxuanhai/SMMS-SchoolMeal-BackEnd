using SMMS.Application.Features.auth.DTOs;
using System.Threading.Tasks;

namespace SMMS.Application.Features.auth.Interfaces
{
    public interface IAuthService
    {
        Task<LoginResponseDto> LoginAsync(LoginRequestDto request);
        Task<LoginResponseDto> RefreshTokenAsync(string refreshToken);
        Task<bool> LogoutAsync(string refreshToken);
        Task ResetFirstPasswordAsync(string email, string currentPassword, string newPassword);
    }
}