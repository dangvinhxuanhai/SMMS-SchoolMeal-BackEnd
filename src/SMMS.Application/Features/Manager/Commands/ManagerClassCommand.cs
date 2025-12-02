using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediatR;
using SMMS.Application.Features.Manager.DTOs;

namespace SMMS.Application.Features.Manager.Commands;
public record CreateClassCommand(CreateClassRequest Request)
    : IRequest<ClassDto>;

// 🟠 Cập nhật lớp
public record UpdateClassCommand(Guid ClassId, UpdateClassRequest Request)
    : IRequest<ClassDto?>;

// 🔴 Xóa lớp
public record DeleteClassCommand(Guid ClassId)
    : IRequest<bool>;
public record CreateAcademicYearCommand(CreateAcademicYearRequest Request)
        : IRequest<AcademicYearDto>;

public record UpdateAcademicYearCommand(int YearId, UpdateAcademicYearRequest Request)
    : IRequest<AcademicYearDto?>;

public record DeleteAcademicYearCommand(int YearId)
    : IRequest<bool>;
