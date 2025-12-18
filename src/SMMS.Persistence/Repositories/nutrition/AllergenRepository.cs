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

    public async Task AddStudentAllergyAsync(
     Guid userId,
     Guid studentId,
     AddStudentAllergyDTO dto)
    {
        var schoolId = await _context.Students
            .Where(s => s.StudentId == studentId)
            .Select(s => s.SchoolId)
            .FirstOrDefaultAsync();

        if (schoolId == Guid.Empty)
            throw new Exception("Student not found");

        int allergenId;

        if (dto.AllergenId.HasValue)
        {
            allergenId = dto.AllergenId.Value;
        }
        else
        {
            if (string.IsNullOrWhiteSpace(dto.AllergenName))
                throw new ArgumentException("AllergenName is required");

            var allergen = new Allergen
            {
                AllergenName = dto.AllergenName,
                SchoolId = schoolId,
                CreatedBy = userId,
                CreatedAt = DateTime.UtcNow,
                AllergenInfo = dto.AllergenInfo,
            };

            _context.Allergens.Add(allergen);
            await _context.SaveChangesAsync();

            allergenId = allergen.AllergenId;
        }
    }
    public async Task<List<IngredientDto>> Search(string keyword)
    {
        return await _context.Ingredients
            .Where(i => i.IngredientName.Contains(keyword))
            .Take(10)
            .Select(i => new IngredientDto
            {
                IngredientId = i.IngredientId,
                IngredientName = i.IngredientName
            })
            .ToListAsync();
    }
    public async Task<List<AllergenDTO>> GetTopAsync(Guid studentId, int top = 5)
    {
        var schoolId = await _context.Students
            .Where(s => s.StudentId == studentId)
            .Select(s => s.SchoolId)
            .FirstAsync();

        return await _context.Allergens
            .Where(a => a.SchoolId == schoolId)
            .OrderByDescending(a => a.AllergeticIngredients.Count)
            .Take(top)
            .Select(a => new AllergenDTO
            {
                AllergenId = a.AllergenId,
                AllergenName = a.AllergenName
            })
            .ToListAsync();
    }
}
