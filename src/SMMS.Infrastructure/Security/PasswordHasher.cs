using Microsoft.AspNetCore.Identity;
using SMMS.Application.Common.Interfaces;

namespace SMMS.Infrastructure.Security
{
    public class PasswordHasher : IPasswordHasher
    {
        private  readonly PasswordHasher<object> _hasher = new PasswordHasher<object>();

        public string HashPassword(string password)
        {
            return _hasher.HashPassword(null, password);
        }

        public bool VerifyPassword(string plainPassword, string hashedPassword)
        {
            var result = _hasher.VerifyHashedPassword(null, hashedPassword, plainPassword);
            return result == PasswordVerificationResult.Success ||
                   result == PasswordVerificationResult.SuccessRehashNeeded;
        }
    }
}
