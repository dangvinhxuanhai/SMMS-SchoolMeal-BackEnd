using SMMS.Domain.Entities.auth;
using System;
using System.Collections.Generic;
using System.Security.Claims;

namespace SMMS.Application.Features.auth.Interfaces
{
    public interface IJwtService
    {
        string GenerateToken(User user, string roleName);
        string GenerateRefreshToken();
        ClaimsPrincipal GetPrincipalFromExpiredToken(string token);
    }
}
