using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SMMS.Domain.Entities.school;

namespace SMMS.Application.Features.Manager.Interfaces;
public interface IManagerClassRepository
{
    IQueryable<Class> Classes { get; }
    IQueryable<AcademicYear> AcademicYears { get; }
    IQueryable<Teacher> Teachers { get; }
    Task<Class?> GetByIdAsync(Guid classId);
    Task AddAsync(Class entity);
    Task UpdateAsync(Class entity);
    Task DeleteAsync(Class entity);
}
