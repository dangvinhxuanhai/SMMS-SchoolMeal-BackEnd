using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SMMS.Application.Features.Manager.Interfaces;
using SMMS.Domain.Entities.school;
using SMMS.Persistence.Data;
using Microsoft.EntityFrameworkCore;
namespace SMMS.Persistence.Repositories.Manager;
public class ManagerAcademicYearRepository : IManagerAcademicYearRepository
{
    private readonly EduMealContext _context;

    public ManagerAcademicYearRepository(EduMealContext context)
    {
        _context = context;
    }

    public IQueryable<AcademicYear> AcademicYears => _context.AcademicYears;

    public async Task<AcademicYear?> GetByIdAsync(int yearId)
        => await _context.AcademicYears.FirstOrDefaultAsync(x => x.YearId == yearId);

    public async Task AddAsync(AcademicYear year)
    {
        _context.AcademicYears.Add(year);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(AcademicYear year)
    {
        _context.AcademicYears.Update(year);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(AcademicYear year)
    {
        _context.AcademicYears.Remove(year);
        await _context.SaveChangesAsync();
    }
}
