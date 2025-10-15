using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SMMS.Application.Features.auth.Interfaces;
using SMMS.Application.Features.Skeleton.Service;
using SMMS.Domain.Models.auth;
using SMMS.Persistence.DbContextSite;

namespace SMMS.Application.Features.auth.Service;
public class UserService : Service<User>, IUserService
{
    public UserService(EduMealContext dbContext) : base(dbContext)
    {
    }
}
