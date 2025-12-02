using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SMMS.Application.Features.auth.Interfaces;
using SMMS.Domain.Entities.auth;
using SMMS.Persistence.Data;
using SMMS.Persistence.Repositories.Skeleton;

namespace SMMS.Persistence.Repositories.auth;
public class UserRepository : Repository<User>, IUserRepository
{
    public UserRepository(EduMealContext dbContext) : base(dbContext)
    {
    }
    public async Task<List<Guid>> GetIdsByRoleAsync(string roleName)
    {
        var targetRole = roleName.ToLower();

        var query = from user in _dbContext.Users
            join role in _dbContext.Roles on user.RoleId equals role.RoleId
            where role.RoleName.ToLower() == targetRole
            select user.UserId;

        return await query.ToListAsync();
    }

    public async Task<List<Guid>> GetAllActiveUserIdsAsync()
    {
        return await _dbContext.Users
            .Where(u => u.IsActive) // Chỉ lấy user còn hoạt động
            .Select(u => u.UserId)  // Chỉ SELECT cột UserId (nhẹ hơn nhiều)
            .ToListAsync();
    }
}
