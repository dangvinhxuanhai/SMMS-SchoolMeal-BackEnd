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
    IRequestHandler<DeleteClassCommand, bool>,
    IRequestHandler<GetAcademicYearsQuery, List<AcademicYearDto>>
{
    private readonly IManagerClassRepository _repo;

    public ManagerClassHandler(IManagerClassRepository repo)
    {
        _repo = repo;
    }

    #region QUERY HANDLERS

    // 🟢 Get all classes by school
    public async Task<List<ClassDto>> Handle(GetAllClassesQuery request, CancellationToken cancellationToken)
    {
        return await _repo.Classes
            .Include(c => c.Teacher).ThenInclude(t => t.TeacherNavigation)
            .Where(c => c.SchoolId == request.SchoolId)
            .OrderByDescending(c => c.CreatedAt)
            .Select(c => new ClassDto
            {
                ClassId = c.ClassId,
                ClassName = c.ClassName,
                SchoolId = c.SchoolId,
                YearId = c.YearId,
                TeacherId = c.TeacherId,
                TeacherName = c.Teacher != null ? c.Teacher.TeacherNavigation.FullName : "(Chưa phân công)",
                IsActive = c.IsActive,
                CreatedAt = c.CreatedAt
            }).ToListAsync(cancellationToken);
    }

    // 🟣 Lấy trạng thái phân công giáo viên
    public async Task<object> Handle(GetTeacherAssignmentStatusQuery request, CancellationToken cancellationToken)
    {
        var allTeachers = await _repo.Teachers
            .Include(t => t.TeacherNavigation)
            .Where(t => t.TeacherNavigation.SchoolId == request.SchoolId)
            .Select(t => new { t.TeacherId, FullName = t.TeacherNavigation.FullName })
            .ToListAsync(cancellationToken);

        var assignedTeachers = await _repo.Classes
            .Where(c => c.TeacherId != null).Select(c => c.TeacherId!.Value)
            .ToListAsync(cancellationToken);

        return new
        {
            TeachersWithClass = allTeachers.Where(t => assignedTeachers.Contains(t.TeacherId)).ToList(),
            TeachersWithoutClass = allTeachers.Where(t => !assignedTeachers.Contains(t.TeacherId)).ToList()
        };
    }

    // 👇 ĐÂY LÀ HÀM BẠN ĐANG THIẾU, HÃY THÊM NÓ VÀO 👇
    // 📅 Lấy danh sách niên khóa
    public async Task<List<AcademicYearDto>> Handle(GetAcademicYearsQuery request, CancellationToken cancellationToken)
    {
        return await _repo.AcademicYears
            .Where(y => y.SchoolId == request.SchoolId)
            .OrderByDescending(y => y.YearName) // Sắp xếp năm mới nhất lên đầu
            .Select(y => new AcademicYearDto
            {
                YearId = y.YearId,
                YearName = y.YearName,
                BoardingStartDate = y.BoardingStartDate,
                BoardingEndDate = y.BoardingEndDate
            })
            .ToListAsync(cancellationToken);
    }

    #endregion

    #region COMMAND HANDLERS

    // 🟡 Create
    public async Task<ClassDto> Handle(
        CreateClassCommand command,
        CancellationToken cancellationToken)
    {
        var request = command.Request;

        // 0. Check niên khóa có thuộc trường này không
        var yearExists = await _repo.AcademicYears
            .AnyAsync(y => y.YearId == request.YearId
                           && y.SchoolId == request.SchoolId,
                      cancellationToken);

        if (!yearExists)
            throw new InvalidOperationException(
                $"Niên khóa (ID: {request.YearId}) không tồn tại trong trường này.");

        // Chuẩn hóa tên lớp
        var normalizedName = request.ClassName.Trim().ToLower();

        // 1. Không cho trùng tên lớp trong CÙNG niên khóa
        var isDuplicateName = await _repo.Classes.AnyAsync(
            c => c.SchoolId == request.SchoolId
                 && c.YearId == request.YearId          // 👈 chỉ check trong cùng niên khóa
                 && c.IsActive
                 && c.ClassName.ToLower() == normalizedName,
            cancellationToken);

        if (isDuplicateName)
        {
            throw new InvalidOperationException(
                $"Lớp tên '{request.ClassName}' đã tồn tại trong niên khóa này rồi!");
        }

        // 2. Check giáo viên đã chủ nhiệm lớp khác chưa
        if (request.TeacherId.HasValue)
        {
            var isTeacherBusy = await _repo.Classes.AnyAsync(
                c => c.TeacherId == request.TeacherId
                     && c.IsActive,
                cancellationToken);

            if (isTeacherBusy)
                throw new InvalidOperationException("Giáo viên này đang chủ nhiệm lớp khác!");
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
    public async Task<ClassDto?> Handle(UpdateClassCommand command, CancellationToken cancellationToken)
    {
        var request = command.Request;
        var entity = await _repo.GetByIdAsync(command.ClassId);
        if (entity == null) return null;

        if (!string.IsNullOrWhiteSpace(request.ClassName)) entity.ClassName = request.ClassName.Trim();

        if (request.TeacherId.HasValue)
        {
            if (entity.TeacherId != request.TeacherId)
            {
                var isTeacherBusy = await _repo.Classes.AnyAsync(c => c.TeacherId == request.TeacherId && c.IsActive,
                    cancellationToken);
                if (isTeacherBusy) throw new InvalidOperationException("Giáo viên này đang chủ nhiệm lớp khác!");
            }

            entity.TeacherId = request.TeacherId;
        }

        if (request.IsActive.HasValue) entity.IsActive = request.IsActive.Value;
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
    public async Task<bool> Handle(DeleteClassCommand command, CancellationToken cancellationToken)
    {
        var entity = await _repo.GetByIdAsync(command.ClassId);
        if (entity == null) return false;
        await _repo.DeleteAsync(entity);
        return true;
    }

    #endregion
}
