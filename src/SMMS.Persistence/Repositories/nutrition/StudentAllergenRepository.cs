using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SMMS.Application.Features.nutrition.Interfaces;
using SMMS.Domain.Models.nutrition;
using SMMS.Persistence.DbContextSite;
using SMMS.Persistence.Repositories.Skeleton;

namespace SMMS.Persistence.Repositories.nutrition;
public class StudentAllergenService : Repository<StudentAllergen>, IStudentAllergenRepository
{
    public StudentAllergenService(EduMealContext dbContext) : base(dbContext)
    {
    }
}
