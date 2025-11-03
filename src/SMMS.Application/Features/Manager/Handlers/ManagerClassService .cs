using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SMMS.Application.Features.Manager.DTOs;
using SMMS.Application.Features.Manager.Interfaces;
using SMMS.Domain.Entities.school;
using Microsoft.EntityFrameworkCore;
namespace SMMS.Application.Features.Manager.Handlers;
public class ManagerClassService : IManagerClassService
{
    private readonly IManagerClassRepository _repo;

    public ManagerClassService(IManagerClassRepository repo)
    {
        _repo = repo;
    }

    // 🟢 Get all classes by school
    public async Task<List<ClassDto>> GetAllAsync(Guid schoolId)
    {
        return await _repo.Classes
            .Where(c => c.SchoolId == schoolId)
            .OrderByDescending(c => c.CreatedAt)
            .Select(c => new ClassDto
            {
                ClassId = c.ClassId,
                ClassName = c.ClassName,
                SchoolId = c.SchoolId,
                YearId = c.YearId,
                TeacherId = c.TeacherId,
                IsActive = c.IsActive,
                CreatedAt = c.CreatedAt
            })
            .ToListAsync();
    }

    // 🟡 Create
    public async Task<ClassDto> CreateAsync(CreateClassRequest request)
    {
        // 🔹 Kiểm tra trùng giáo viên
        if (request.TeacherId.HasValue)
        {
            bool teacherAssigned = await _repo.Classes.AnyAsync(c => c.TeacherId == request.TeacherId);
            if (teacherAssigned)
                throw new InvalidOperationException("Giáo viên này đã được gán cho một lớp khác.");
        }

        var newClass = new Class
        {
            ClassId = Guid.NewGuid(),
            ClassName = request.ClassName.Trim(),
            SchoolId = request.SchoolId,
            YearId = request.YearId,
            TeacherId = request.TeacherId,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedBy = request.CreatedBy.HasValue ? (int?)0 : null
        };

        await _repo.AddAsync(newClass);

        return new ClassDto
        {
            ClassId = newClass.ClassId,
            ClassName = newClass.ClassName,
            SchoolId = newClass.SchoolId,
            YearId = newClass.YearId,
            TeacherId = newClass.TeacherId,
            IsActive = newClass.IsActive,
            CreatedAt = newClass.CreatedAt
        };
    }
    // 🟠 Update
    public async Task<ClassDto?> UpdateAsync(Guid classId, UpdateClassRequest request)
    {
        var entity = await _repo.GetByIdAsync(classId);
        if (entity == null) return null;

        if (!string.IsNullOrWhiteSpace(request.ClassName))
            entity.ClassName = request.ClassName.Trim();

        if (request.TeacherId.HasValue)
            entity.TeacherId = request.TeacherId;

        if (request.IsActive.HasValue)
            entity.IsActive = request.IsActive.Value;

        entity.UpdatedAt = DateTime.UtcNow;
        entity.UpdatedBy = request.UpdatedBy.HasValue ? (int?)0 : null;

        await _repo.UpdateAsync(entity);

        return new ClassDto
        {
            ClassId = entity.ClassId,
            ClassName = entity.ClassName,
            SchoolId = entity.SchoolId,
            YearId = entity.YearId,
            TeacherId = entity.TeacherId,
            IsActive = entity.IsActive,
            CreatedAt = entity.CreatedAt
        };
    }

    // 🔴 Delete
    public async Task<bool> DeleteAsync(Guid classId)
    {
        var entity = await _repo.GetByIdAsync(classId);
        if (entity == null)
            return false;

        await _repo.DeleteAsync(entity);
        return true;
    }
}
