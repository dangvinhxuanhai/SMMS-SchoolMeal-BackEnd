using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SMMS.Application.Features.school.Interfaces;
using SMMS.Domain.Models.school;
using SMMS.Persistence.DbContextSite;
using SMMS.Persistence.Repositories.Skeleton;

namespace SMMS.Persistence.Repositories.school;
public class StudentService : Repository<Student>, IStudentRepository
{
    public StudentService(EduMealContext dbContext) : base(dbContext)
    {
    }
}
