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
        // 1️⃣ Lấy SchoolId của học sinh
        var schoolId = await _context.Students
            .Where(s => s.StudentId == studentId)
            .Select(s => s.SchoolId)
            .FirstOrDefaultAsync();

        if (schoolId == Guid.Empty)
            throw new Exception("Student not found");

        // 2️⃣ Lấy Ingredient
        var ingredient = await _context.Ingredients
            .Where(i => i.IngredientId == dto.IngredientId && i.IsActive)
            .FirstOrDefaultAsync();

        if (ingredient == null)
            throw new Exception("Ingredient không tồn tại");

        // 3️⃣ Kiểm tra Allergen đã tồn tại chưa (theo IngredientName + School)
        var allergen = await _context.Allergens
            .FirstOrDefaultAsync(a =>
                a.SchoolId == schoolId &&
                a.AllergenName.ToLower() == ingredient.IngredientName.ToLower());

        if (allergen == null)
        {
            // 4️⃣ Tạo mới Allergen
            allergen = new Allergen
            {
                AllergenName = ingredient.IngredientName,
                AllergenMatter = ingredient.IngredientType ?? "Ingredient",
                SchoolId = schoolId,
                CreatedBy = userId,
                CreatedAt = DateTime.UtcNow,
                AllergenInfo = "Dị ứng từ nguyên liệu"
            };

            _context.Allergens.Add(allergen);
            await _context.SaveChangesAsync(); // để có AllergenId
        }

        // 5️⃣ Kiểm tra đã khai báo dị ứng nguyên liệu này chưa
        var existing = await _context.AllergeticIngredients
            .FirstOrDefaultAsync(ai =>
                ai.IngredientId == ingredient.IngredientId &&
                ai.AllergenId == allergen.AllergenId);

        if (existing != null)
            throw new Exception("Nguyên liệu này đã được khai báo dị ứng");

        // 6️⃣ Lưu vào bảng AllergeticIngredients
        var allergenicIngredient = new AllergeticIngredient
        {
            IngredientId = ingredient.IngredientId,
            AllergenId = allergen.AllergenId,
            ReportedAt = DateTime.UtcNow,
            DiagnosedAt = DateTime.UtcNow,
            Notes = dto.Notes,
            ReactionNotes = dto.ReactionNotes,
            HandlingNotes = dto.HandlingNotes
        };

        _context.AllergeticIngredients.Add(allergenicIngredient);
        await _context.SaveChangesAsync();
    }


    public async Task<List<IngredientDto>> Search(string keyword)
    {
        return await _context.Ingredients
            .Where(i => i.IngredientName.Contains(keyword))
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
            .OrderByDescending(a =>
                a.StudentAllergens.Count) // Sửa thành StudentAllergens.Count để đếm chính xác số HS bị
            .Take(top)
            .Select(a => new AllergenDTO { AllergenId = a.AllergenId, AllergenName = a.AllergenName })
            .ToListAsync();
    }
}
