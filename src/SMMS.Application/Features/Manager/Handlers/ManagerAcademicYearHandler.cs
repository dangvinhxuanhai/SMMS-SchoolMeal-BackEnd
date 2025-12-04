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

        if (req.SchoolId == Guid.Empty)
            throw new InvalidOperationException("Trường học không hợp lệ.");

        if (string.IsNullOrWhiteSpace(req.YearName))
            throw new InvalidOperationException("Tên niên khóa không được để trống.");

        // chuẩn hóa tên
        req.YearName = req.YearName.Trim();
        if (req.YearName.Length > 100)
            throw new InvalidOperationException("Tên niên khóa không được vượt quá 100 ký tự.");

        var normalizedName = req.YearName.ToLower();

        // ❌ Không cho trùng tên trong cùng 1 trường
        var isDuplicate = await _repo.AcademicYears.AnyAsync(
            y => y.SchoolId == req.SchoolId &&
                 y.YearName.ToLower() == normalizedName,
            cancellationToken);

        if (isDuplicate)
            throw new InvalidOperationException(
                $"Niên khóa '{req.YearName}' đã tồn tại trong trường này."
            );

        // validate ngày
        if (req.BoardingStartDate.HasValue ^ req.BoardingEndDate.HasValue)
            throw new InvalidOperationException("Vui lòng nhập đầy đủ cả ngày bắt đầu và ngày kết thúc nội trú.");

        if (req.BoardingStartDate.HasValue && req.BoardingEndDate.HasValue &&
            req.BoardingStartDate > req.BoardingEndDate)
        {
            throw new InvalidOperationException("Ngày bắt đầu không được lớn hơn ngày kết thúc.");
        }

        var entity = new AcademicYear
        {
            YearName = req.YearName,
            BoardingStartDate = req.BoardingStartDate,
            BoardingEndDate = req.BoardingEndDate,
            SchoolId = req.SchoolId
        };

        await _repo.AddAsync(entity);

        return new AcademicYearDto
        {
            YearId = entity.YearId,
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
        var entity = await _repo.GetByIdAsync(command.YearId);
        if (entity == null) return null;

        // ----- xử lý & validate YearName -----
        if (!string.IsNullOrWhiteSpace(req.YearName))
        {
            req.YearName = req.YearName.Trim();

            if (req.YearName.Length > 100)
                throw new InvalidOperationException("Tên niên khóa không được vượt quá 100 ký tự.");

            var normalizedName = req.YearName.ToLower();

            // ❌ Check trùng tên trong cùng trường, khác chính nó
            var isDuplicate = await _repo.AcademicYears.AnyAsync(
                y => y.SchoolId == entity.SchoolId &&
                     y.YearId != entity.YearId &&
                     y.YearName.ToLower() == normalizedName,
                cancellationToken);

            if (isDuplicate)
                throw new InvalidOperationException(
                    $"Niên khóa '{req.YearName}' đã tồn tại trong trường này."
                );

            entity.YearName = req.YearName;
        }

        // ----- xử lý ngày (giữ giá trị cũ nếu không truyền) -----
        var newStart = req.BoardingStartDate.HasValue
            ? req.BoardingStartDate
            : entity.BoardingStartDate;

        var newEnd = req.BoardingEndDate.HasValue
            ? req.BoardingEndDate
            : entity.BoardingEndDate;

        if ((req.BoardingStartDate.HasValue || req.BoardingEndDate.HasValue) &&
            (newStart.HasValue ^ newEnd.HasValue))
        {
            throw new InvalidOperationException("Vui lòng nhập đầy đủ cả ngày bắt đầu và ngày kết thúc nội trú.");
        }

        if (newStart.HasValue && newEnd.HasValue && newStart > newEnd)
            throw new InvalidOperationException("Ngày bắt đầu không được lớn hơn ngày kết thúc.");

        if (req.BoardingStartDate.HasValue)
            entity.BoardingStartDate = req.BoardingStartDate;

        if (req.BoardingEndDate.HasValue)
            entity.BoardingEndDate = req.BoardingEndDate;

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
