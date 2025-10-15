using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SMMS.Application.Features.school.Interfaces;
using SMMS.Application.Features.Skeleton.Service;
using SMMS.Domain.Models.school;
using SMMS.Persistence.DbContextSite;

namespace SMMS.Application.Features.IdentityGroup.Service;
public class StudentService : Service<Student>, IStudentService
{
    public StudentService(EduMealContext dbContext) : base(dbContext)
    {
    }
{
}
