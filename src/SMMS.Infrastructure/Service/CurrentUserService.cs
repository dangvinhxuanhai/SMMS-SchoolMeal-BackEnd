using System;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;
using SMMS.Application.Abstractions;

namespace SMMS.Infrastructure.Services
{
    public class CurrentUserService : ICurrentUserService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public CurrentUserService(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public Guid? UserId
        {
            get
            {
                // ✅ Lấy userId từ claim (nếu có token)
                var user = _httpContextAccessor.HttpContext?.User
                   ?? throw new UnauthorizedAccessException("No HttpContext or User.");

                var userIdString =
                        user.FindFirst("UserId")?.Value
                        ?? user.FindFirst(ClaimTypes.NameIdentifier)?.Value
                        ?? user.FindFirst("sub")?.Value
                        ?? user.FindFirst("id")?.Value;

                return Guid.TryParse(userIdString, out var id) ? id : null;
            }
        }

        public string? Email => _httpContextAccessor.HttpContext?.User?.FindFirst("email")?.Value;
    }
}
