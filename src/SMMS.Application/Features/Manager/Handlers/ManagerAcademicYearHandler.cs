using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using SMMS.Application.Features.Manager.Commands;
using SMMS.Application.Features.Manager.DTOs;
using SMMS.Application.Features.Manager.Interfaces;
using SMMS.Application.Features.Manager.Queries;
using SMMS.Domain.Entities.school;

namespace SMMS.Application.Features.Manager.Handlers;
public class ManagerAcademicYearHandler :
        IRequestHandler<GetAcademicYearByIdQuery, AcademicYearDto?>,
        IRequestHandler<CreateAcademicYearCommand, AcademicYearDto>,
        IRequestHandler<UpdateAcademicYearCommand, AcademicYearDto?>,
        IRequestHandler<DeleteAcademicYearCommand, bool>
{
    private readonly IManagerAcademicYearRepository _repo;

    public ManagerAcademicYearHandler(IManagerAcademicYearRepository repo)
    {
        _repo = repo;
    }


    // 🔍 Lấy chi tiết 1 niên khóa
    public async Task<AcademicYearDto?> Handle(
        GetAcademicYearByIdQuery request,
        CancellationToken cancellationToken)
    {
        var entity = await _repo.GetByIdAsync(request.YearId);
        if (entity == null) return null;

        return new AcademicYearDto
        {
            YearId = entity.YearId,
            YearName = entity.YearName,
            BoardingStartDate = entity.BoardingStartDate,
            BoardingEndDate = entity.BoardingEndDate,
            SchoolId = entity.SchoolId
        };
    }

    // 🟡 Tạo niên khóa
    public async Task<AcademicYearDto> Handle(
        CreateAcademicYearCommand command,
        CancellationToken cancellationToken)
    {
        var req = command.Request;

        if (string.IsNullOrWhiteSpace(req.YearName))
            throw new InvalidOperationException("Tên niên khóa không được để trống.");

        var normalizedName = req.YearName.Trim().ToLower();

        var isDuplicate = await _repo.AcademicYears.AnyAsync(
            y => y.SchoolId == req.SchoolId &&
                 y.YearName.ToLower() == normalizedName,
            cancellationToken);

        if (isDuplicate)
            throw new InvalidOperationException($"Niên khóa '{req.YearName}' đã tồn tại trong trường này.");

        if (req.BoardingStartDate.HasValue && req.BoardingEndDate.HasValue &&
            req.BoardingStartDate > req.BoardingEndDate)
        {
            throw new InvalidOperationException("Ngày bắt đầu không được lớn hơn ngày kết thúc.");
        }

        var entity = new AcademicYear
        {
            // ❌ KHÔNG YearId = Guid.NewGuid();
            YearName = req.YearName.Trim(),
            BoardingStartDate = req.BoardingStartDate,
            BoardingEndDate = req.BoardingEndDate,
            SchoolId = req.SchoolId
        };

        await _repo.AddAsync(entity); // Sau SaveChanges, entity.YearId (int) sẽ được DB set

        return new AcademicYearDto
        {
            YearId = entity.YearId,   // int
            YearName = entity.YearName,
            BoardingStartDate = entity.BoardingStartDate,
            BoardingEndDate = entity.BoardingEndDate,
            SchoolId = entity.SchoolId
        };
    }

    // 🟠 Cập nhật niên khóa
    public async Task<AcademicYearDto?> Handle(
        UpdateAcademicYearCommand command,
        CancellationToken cancellationToken)
    {
        var req = command.Request;
        var entity = await _repo.GetByIdAsync(command.YearId); // command.YearId: int
        if (entity == null) return null;

        if (!string.IsNullOrWhiteSpace(req.YearName))
        {
            var normalizedName = req.YearName.Trim().ToLower();

            var isDuplicate = await _repo.AcademicYears.AnyAsync(
                y => y.SchoolId == entity.SchoolId &&
                     y.YearId != entity.YearId &&                 // int
                     y.YearName.ToLower() == normalizedName,
                cancellationToken);

            if (isDuplicate)
                throw new InvalidOperationException($"Niên khóa '{req.YearName}' đã tồn tại trong trường này.");

            entity.YearName = req.YearName.Trim();
        }

        if (req.BoardingStartDate.HasValue)
            entity.BoardingStartDate = req.BoardingStartDate.Value;

        if (req.BoardingEndDate.HasValue)
            entity.BoardingEndDate = req.BoardingEndDate.Value;

        if (entity.BoardingStartDate.HasValue && entity.BoardingEndDate.HasValue &&
            entity.BoardingStartDate > entity.BoardingEndDate)
        {
            throw new InvalidOperationException("Ngày bắt đầu không được lớn hơn ngày kết thúc.");
        }

        await _repo.UpdateAsync(entity);

        return new AcademicYearDto
        {
            YearId = entity.YearId,
            YearName = entity.YearName,
            BoardingStartDate = entity.BoardingStartDate,
            BoardingEndDate = entity.BoardingEndDate,
            SchoolId = entity.SchoolId
        };
    }


    // 🔴 Xoá niên khóa
    public async Task<bool> Handle(
        DeleteAcademicYearCommand command,
        CancellationToken cancellationToken)
    {
        var entity = await _repo.GetByIdAsync(command.YearId); // int
        if (entity == null) return false;

        await _repo.DeleteAsync(entity);
        return true;
    }

}
