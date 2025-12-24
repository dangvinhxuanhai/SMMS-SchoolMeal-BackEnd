using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SMMS.Application.Features.nutrition.DTOs;
using SMMS.Application.Features.nutrition.Interfaces;
using SMMS.Domain.Entities.nutrition;
using SMMS.Persistence.Data;

namespace SMMS.Persistence.Repositories.nutrition;

public class AllergenRepository : IAllergenRepository
{
    private readonly EduMealContext _context;
    public AllergenRepository(EduMealContext context) => _context = context;

    public async Task<List<AllergenDTO>> GetAllAsync(Guid studentId)
    {
        return await _context.StudentAllergens
            .Where(sa => sa.StudentId == studentId)
            .OrderByDescending(sa => sa.Allergen.CreatedAt)
            .Select(sa => new AllergenDTO
            {
                AllergenId = sa.AllergenId,
                AllergenName = sa.Allergen.AllergenName,
                AllergenMatter = sa.Allergen.AllergenMatter,
                AllergenInfo = sa.Allergen.AllergenInfo,
                CreatedAt = sa.Allergen.CreatedAt,
                ReactionNotes = sa.ReactionNotes,
                Notes = sa.Notes,
            })
            .ToListAsync();
    }

    public async Task AddStudentAllergyAsync(Guid userId, Guid studentId, AddStudentAllergyDTO dto)
    {
        var student = await _context.Students.FirstOrDefaultAsync(s => s.StudentId == studentId);
        if (student == null) throw new Exception("Không tìm thấy học sinh.");
        var schoolId = student.SchoolId;

        var ingredient = await _context.Ingredients
            .FirstOrDefaultAsync(i => i.IngredientId == dto.IngredientId && i.IsActive);
        if (ingredient == null) throw new Exception("Ingredient không tồn tại hoặc đã bị ẩn.");

        var allergen = await _context.Allergens
            .FirstOrDefaultAsync(a => a.SchoolId == schoolId &&
                                      a.AllergenName.ToLower() == ingredient.IngredientName.ToLower());

        if (allergen == null)
        {
            allergen = new Allergen
            {
                AllergenName = ingredient.IngredientName,
                AllergenMatter = ingredient.IngredientType ?? "Ingredient",
                SchoolId = schoolId,
                CreatedBy = userId,
                CreatedAt = DateTime.UtcNow,
                AllergenInfo = $"Dị ứng tự động tạo từ: {ingredient.IngredientName}"
            };
            _context.Allergens.Add(allergen);
            await _context.SaveChangesAsync(); // Lưu để lấy AllergenId
        }

        var alreadyLinked = await _context.StudentAllergens
            .AnyAsync(sa => sa.StudentId == studentId && sa.AllergenId == allergen.AllergenId);
        if (alreadyLinked) throw new Exception("Học sinh này đã được khai báo dị ứng này rồi.");

        var studentAllergen = new StudentAllergen
        {
            StudentId = studentId,
            AllergenId = allergen.AllergenId,
            Notes = dto.Notes,
            ReactionNotes = dto.ReactionNotes,
            HandlingNotes = dto.HandlingNotes,
            DiagnosedAt = DateTime.UtcNow
        };
        _context.StudentAllergens.Add(studentAllergen);

        var isLinkExist = await _context.AllergeticIngredients
            .AnyAsync(ai => ai.IngredientId == ingredient.IngredientId && ai.AllergenId == allergen.AllergenId);

        if (!isLinkExist)
        {
            _context.AllergeticIngredients.Add(new AllergeticIngredient
            {
                IngredientId = ingredient.IngredientId,
                AllergenId = allergen.AllergenId,
                ReportedAt = DateTime.UtcNow
            });
        }

        await _context.SaveChangesAsync();
    }

    public async Task<List<IngredientDto>> Search(string keyword)
    {
        return await _context.Ingredients
            .Where(i => i.IngredientName.Contains(keyword) && i.IsActive)
            .Take(10)
            .Select(i => new IngredientDto { IngredientId = i.IngredientId, IngredientName = i.IngredientName })
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
            .OrderByDescending(a => a.StudentAllergens.Count)
            .Take(top)
            .Select(a => new AllergenDTO { AllergenId = a.AllergenId, AllergenName = a.AllergenName })
            .ToListAsync();
    }
}
