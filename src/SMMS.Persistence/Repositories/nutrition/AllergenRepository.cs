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

    public async Task AddStudentAllergyAsync(Guid userId, Guid studentId, AddStudentAllergyDTO dto)
    {
        var schoolId = await _context.Students
            .Where(s => s.StudentId == studentId)
            .Select(s => s.SchoolId)
            .FirstOrDefaultAsync();

        if (schoolId == Guid.Empty)
            throw new Exception("Student not found");

        int finalAllergenId;

        if (dto.AllergenId.HasValue && dto.AllergenId.Value > 0)
        {
            var exists = await _context.Allergens.AnyAsync(a => a.AllergenId == dto.AllergenId.Value);
            if (!exists) throw new KeyNotFoundException("Allergen ID không tồn tại.");

            finalAllergenId = dto.AllergenId.Value;
        }
        else
        {
            if (string.IsNullOrWhiteSpace(dto.AllergenName))
                throw new ArgumentException("Tên dị ứng không được để trống khi chọn mục Khác.");

            string cleanName = dto.AllergenName.Trim();
            if (cleanName.StartsWith("Khác:", StringComparison.OrdinalIgnoreCase))
            {
                cleanName = cleanName.Substring(5).Trim();
            }
            else if (cleanName.StartsWith("Khác", StringComparison.OrdinalIgnoreCase))
            {
                cleanName = cleanName.Replace("Khác", "", StringComparison.OrdinalIgnoreCase).Trim();
            }

            if (string.IsNullOrWhiteSpace(cleanName))
                throw new ArgumentException("Vui lòng nhập tên loại dị ứng cụ thể.");

            var existingAllergen = await _context.Allergens
                .Where(a => a.SchoolId == schoolId && a.AllergenName.ToLower() == cleanName.ToLower())
                .FirstOrDefaultAsync();

            if (existingAllergen != null)
            {
                finalAllergenId = existingAllergen.AllergenId;
            }
            else
            {
                var newAllergen = new Allergen
                {
                    AllergenName = cleanName,
                    SchoolId = schoolId,
                    CreatedBy = userId,
                    CreatedAt = DateTime.UtcNow,
                    AllergenInfo = dto.AllergenInfo ?? "Dị ứng khác",
                    AllergenMatter = "Khác"
                };

                _context.Allergens.Add(newAllergen);
                await _context.SaveChangesAsync();

                finalAllergenId = newAllergen.AllergenId;
            }
        }

        var existingLink = await _context.StudentAllergens
            .FirstOrDefaultAsync(sa => sa.StudentId == studentId && sa.AllergenId == finalAllergenId);

        if (existingLink == null)
        {
            var studentAllergen = new StudentAllergen
            {
                StudentId = studentId, AllergenId = finalAllergenId, DiagnosedAt = DateTime.UtcNow
            };

            _context.StudentAllergens.Add(studentAllergen);
            await _context.SaveChangesAsync();
        }
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
