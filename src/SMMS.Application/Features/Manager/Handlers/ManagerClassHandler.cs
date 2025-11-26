using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediatR;
using SMMS.Application.Features.Manager.Commands;
using SMMS.Application.Features.Manager.Interfaces;
using SMMS.Application.Features.Manager.Queries;
using SMMS.Domain.Entities.school;
using Microsoft.EntityFrameworkCore;
using SMMS.Application.Features.Manager.DTOs;

namespace SMMS.Application.Features.Manager.Handlers;
public class ManagerClassHandler :
    IRequestHandler<GetAllClassesQuery, List<ClassDto>>,
    IRequestHandler<GetTeacherAssignmentStatusQuery, object>,
    IRequestHandler<CreateClassCommand, ClassDto>,
    IRequestHandler<UpdateClassCommand, ClassDto?>,
    IRequestHandler<DeleteClassCommand, bool>
{
    private readonly IManagerClassRepository _repo;
    public ManagerClassHandler(IManagerClassRepository repo)
    {
        _repo = repo;
    }

    #region QUERY HANDLERS

    // 🟢 Get all classes by school
    public async Task<List<ClassDto>> Handle(
        GetAllClassesQuery request,
        CancellationToken cancellationToken)
    {
        return await _repo.Classes
            .Include(c => c.Teacher)
            .ThenInclude(t => t.TeacherNavigation)
            .Where(c => c.SchoolId == request.SchoolId)
            .OrderByDescending(c => c.CreatedAt)
            .Select(c => new ClassDto
            {
                ClassId = c.ClassId,
                ClassName = c.ClassName,
                SchoolId = c.SchoolId,
                YearId = c.YearId,
                TeacherId = c.TeacherId,
                TeacherName = c.Teacher != null
                    ? c.Teacher.TeacherNavigation.FullName
                    : "(Chưa phân công)",
                IsActive = c.IsActive,
                CreatedAt = c.CreatedAt
            })
            .ToListAsync(cancellationToken);
    }

    // 🟣 Lấy trạng thái phân công giáo viên
    public async Task<object> Handle(
        GetTeacherAssignmentStatusQuery request,
        CancellationToken cancellationToken)
    {
        // 🟢 Lấy toàn bộ giáo viên trong school
        var allTeachers = await _repo.Teachers
            .Include(t => t.TeacherNavigation)
            .Where(t => t.TeacherNavigation.SchoolId == request.SchoolId)
            .Select(t => new
            {
                t.TeacherId,
                FullName = t.TeacherNavigation.FullName
            })
            .ToListAsync(cancellationToken);

        // 🟡 Lấy danh sách giáo viên đã được phân lớp
        var assignedTeachers = await _repo.Classes
            .Where(c => c.TeacherId != null)
            .Select(c => c.TeacherId!.Value)
            .ToListAsync(cancellationToken);

        // 🟣 Phân loại
        var teachersWithClass = allTeachers
            .Where(t => assignedTeachers.Contains(t.TeacherId))
            .ToList();

        var teachersWithoutClass = allTeachers
            .Where(t => !assignedTeachers.Contains(t.TeacherId))
            .ToList();

        // 🔹 Trả kết quả (giữ object + anonymous type giống service cũ)
        return new
        {
            TeachersWithClass = teachersWithClass,
            TeachersWithoutClass = teachersWithoutClass
        };
    }

    #endregion

    #region COMMAND HANDLERS

    // 🟡 Create
    public async Task<ClassDto> Handle(
        CreateClassCommand command,
        CancellationToken cancellationToken)
    {
        var request = command.Request;

        // 🔹 Kiểm tra trùng giáo viên
        if (request.TeacherId.HasValue)
        {
            bool teacherAssigned = await _repo.Classes
                .AnyAsync(c => c.TeacherId == request.TeacherId, cancellationToken);

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
            // logic cũ của bạn, giữ nguyên
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
    public async Task<ClassDto?> Handle(
        UpdateClassCommand command,
        CancellationToken cancellationToken)
    {
        var request = command.Request;
        var entity = await _repo.GetByIdAsync(command.ClassId);
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
    public async Task<bool> Handle(
        DeleteClassCommand command,
        CancellationToken cancellationToken)
    {
        var entity = await _repo.GetByIdAsync(command.ClassId);
        if (entity == null)
            return false;

        await _repo.DeleteAsync(entity);
        return true;
    }

    #endregion
}
