using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SMMS.Domain.Entities.school;

namespace SMMS.Application.Features.Manager.Interfaces;
public interface IManagerAcademicYearRepository
{
    IQueryable<AcademicYear> AcademicYears { get; }

    Task<AcademicYear?> GetByIdAsync(int yearId);
    Task AddAsync(AcademicYear year);
    Task UpdateAsync(AcademicYear year);
    Task DeleteAsync(AcademicYear year);
}
