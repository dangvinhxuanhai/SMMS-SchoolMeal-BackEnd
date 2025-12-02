using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SMMS.Application.Features.Wardens.Interfaces;
using SMMS.Domain.Entities.school;
using SMMS.Persistence.Data;

namespace SMMS.Persistence.Repositories.Wardens;
public class CloudStorageRepository : ICloudStorageRepository
{
    private readonly EduMealContext _context;

    public CloudStorageRepository(EduMealContext context)
    {
        _context = context;
    }

    public IQueryable<Student> Students => _context.Students;
    public IQueryable<StudentClass> StudentClasses => _context.StudentClasses;
    public IQueryable<Class> Classes => _context.Classes;
    public IQueryable<AcademicYear> AcademicYears => _context.AcademicYears;
    public IQueryable<School> Schools => _context.Schools;
    public IQueryable<StudentImage> StudentImages => _context.StudentImages;
}
