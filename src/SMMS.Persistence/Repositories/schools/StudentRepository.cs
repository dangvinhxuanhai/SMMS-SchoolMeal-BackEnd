using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SMMS.Application.Features.school.Interfaces;
using SMMS.Domain.Entities.school;
using SMMS.Persistence.Data;
using SMMS.Persistence.Repositories.Skeleton;

namespace SMMS.Persistence.Repositories.schools;
public class StudentService : Repository<Student>, IStudentRepository
{
    public StudentService(EduMealContext dbContext) : base(dbContext)
    {
    }
}
