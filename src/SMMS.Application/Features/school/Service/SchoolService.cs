using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SMMS.Application.Features.school.Interfaces;
using SMMS.Application.Features.Skeleton.Service;
using SMMS.Domain.Models.school;
using SMMS.Persistence.DbContextSite;

namespace SMMS.Application.Features.school.Service;
public class SchoolService : Service<School>, ISchoolService
{
    public SchoolService(EduMealContext dbContext) : base(dbContext)
    {
    }
}
