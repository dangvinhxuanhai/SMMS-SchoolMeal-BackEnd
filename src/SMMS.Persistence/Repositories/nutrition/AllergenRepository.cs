using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SMMS.Application.Features.nutrition.DTOs;
using SMMS.Application.Features.nutrition.Interfaces;
using SMMS.Domain.Entities.nutrition;
using SMMS.Domain.Entities.school;
using SMMS.Persistence.Data;
using SMMS.Persistence.Repositories.Skeleton;

namespace SMMS.Persistence.Repositories.nutrition;
public class AllergenRepository : IAllergenRepository
{
    private readonly EduMealContext _context;

    public AllergenRepository(EduMealContext context)
    {
        _context = context;
    }

    public async Task<List<AllergenDTO>> GetAllAsync(Guid studentId)
    {
        var schoolId = await _context.Students
            .Where(s => s.StudentId == studentId)
            .Select(s => s.SchoolId)
            .FirstOrDefaultAsync();
        return await _context.Allergens
            .Where(a => a.SchoolId == schoolId)
            .OrderByDescending(a => a.CreatedAt)
            .Select(a => new AllergenDTO
            {
                AllergenId = a.AllergenId,
                AllergenName = a.AllergenName,
                AllergenMatter = a.AllergenMatter,
                AllergenInfo = a.AllergenInfo,
                CreatedAt = a.CreatedAt
            })
            .ToListAsync();
    }
}
