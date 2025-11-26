using Microsoft.AspNetCore.Identity;

namespace SMMS.Infrastructure.Security
{
    public static class PasswordHasher
    {
        private static readonly PasswordHasher<object> _hasher = new PasswordHasher<object>();

        // Hash mật khẩu theo chuẩn ASP.NET Identity
        public static string HashPassword(string password)
        {
            return _hasher.HashPassword(null, password);
        }

        // Kiểm tra mật khẩu có khớp với hash không
        public static bool VerifyPassword(string plainPassword, string hashedPassword)
        {
            var result = _hasher.VerifyHashedPassword(null, hashedPassword, plainPassword);
            return result == PasswordVerificationResult.Success ||
                   result == PasswordVerificationResult.SuccessRehashNeeded;
        }
    }
}
