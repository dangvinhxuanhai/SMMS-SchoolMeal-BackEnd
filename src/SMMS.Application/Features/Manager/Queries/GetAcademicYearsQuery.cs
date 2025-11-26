using MediatR;
using SMMS.Application.Features.Manager.DTOs;

namespace SMMS.Application.Features.Manager.Queries;

    public record GetAcademicYearsQuery(Guid SchoolId) : IRequest<List<AcademicYearDto>>;

