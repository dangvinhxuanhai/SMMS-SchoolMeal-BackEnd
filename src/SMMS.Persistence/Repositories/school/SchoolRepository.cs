using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SMMS.Application.Features.school.Interfaces;
using SMMS.Domain.Entities.school;
using SMMS.Persistence.DbContextSite;
using SMMS.Persistence.Repositories.Skeleton;

namespace SMMS.Persistence.Repositories.school;
public class SchoolService : Repository<School>, ISchoolRepository
{
    public SchoolService(EduMealContext dbContext) : base(dbContext)
    {
    }
}
