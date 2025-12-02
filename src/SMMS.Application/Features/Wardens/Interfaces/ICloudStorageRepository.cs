using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SMMS.Domain.Entities.school;

namespace SMMS.Application.Features.Wardens.Interfaces;
public interface ICloudStorageRepository
{
    IQueryable<Student> Students { get; }
    IQueryable<StudentClass> StudentClasses { get; }
    IQueryable<Class> Classes { get; }
    IQueryable<AcademicYear> AcademicYears { get; }
    IQueryable<School> Schools { get; }
    IQueryable<StudentImage> StudentImages { get; }
}
