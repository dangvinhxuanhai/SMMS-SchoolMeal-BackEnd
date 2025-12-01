using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SMMS.Application.Features.Skeleton.Interfaces;
using SMMS.Domain.Entities.auth;

namespace SMMS.Application.Features.auth.Interfaces;
public interface IUserRepository : IRepository<User>
{
    Task<List<Guid>> GetIdsByRoleAsync(string roleName);
    Task<List<Guid>> GetAllActiveUserIdsAsync();
}
