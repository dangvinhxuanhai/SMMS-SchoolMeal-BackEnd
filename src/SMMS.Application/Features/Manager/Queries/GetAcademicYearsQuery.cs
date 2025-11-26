using MediatR;
using SMMS.Application.Features.Manager.DTOs;

namespace SMMS.Application.Features.Manager.Queries;

// 👇 Xóa class bao ngoài, chỉ để lại dòng này thôi
public record GetAcademicYearsQuery(Guid SchoolId) : IRequest<List<AcademicYearDto>>;
