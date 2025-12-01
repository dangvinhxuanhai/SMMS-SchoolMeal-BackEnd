using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediatR;
using SMMS.Application.Common.Interfaces;
using SMMS.Application.Features.Menus.DTOs.Schooling;
using SMMS.Application.Features.Menus.Queries.Schooling;
using SMMS.Domain.Entities.school;

namespace SMMS.Application.Features.Menus.Handlers.Schooling;
public sealed class GetSchoolByIdQueryHandler : IRequestHandler<GetSchoolByIdQuery, SchoolDetailDto?>
{
    private readonly IReadRepository<School, Guid> _repo;

    public GetSchoolByIdQueryHandler(IReadRepository<School, Guid> repo)
    {
        _repo = repo;
    }

    public async Task<SchoolDetailDto?> Handle(GetSchoolByIdQuery request, CancellationToken ct)
    {
        var entity = await _repo.GetByIdAsync(
            request.Id,
            keyName: nameof(School.SchoolId),
            ct
        );

        if (entity is null) return null;

        return new SchoolDetailDto
        {
            SchoolId = entity.SchoolId,
            SchoolName = entity.SchoolName,
            ContactEmail = entity.ContactEmail,
            Hotline = entity.Hotline,
            //SchoolContract = entity.SchoolContract,
            SchoolAddress = entity.SchoolAddress,
            IsActive = entity.IsActive,
            CreatedAt = entity.CreatedAt,
            CreatedBy = entity.CreatedBy,
            UpdatedAt = entity.UpdatedAt,
            UpdatedBy = entity.UpdatedBy
        };
    }
}
