using System;
using Microsoft.AspNetCore.Http;
using SMMS.Application.Features.Identity.Interfaces;

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
                var userId = _httpContextAccessor.HttpContext?.User?.FindFirst("userId")?.Value;

                // ✅ Nếu chưa đăng nhập thì dùng ID thật bạn cung cấp
                if (string.IsNullOrEmpty(userId))
                    return Guid.Parse("736CB98F-15BE-4E38-80C0-02066F3B6755");

                return Guid.Parse(userId);
            }
        }

        public string? Email => _httpContextAccessor.HttpContext?.User?.FindFirst("email")?.Value;
    }
}
