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
public sealed class GetAllSchoolsQueryHandler
    : IRequestHandler<GetAllSchoolsQuery, IReadOnlyList<SchoolListItemDto>>
{
    private readonly IReadRepository<School, Guid> _repo;

    public GetAllSchoolsQueryHandler(IReadRepository<School, Guid> repo)
    {
        _repo = repo;
    }

    public async Task<IReadOnlyList<SchoolListItemDto>> Handle(GetAllSchoolsQuery request, CancellationToken ct)
    {
        var entities = await _repo.GetAllAsync(ct);

        return entities
            .Select(e => new SchoolListItemDto
            {
                SchoolId = e.SchoolId,
                SchoolName = e.SchoolName,
                ContactEmail = e.ContactEmail,
                Hotline = e.Hotline,
                IsActive = e.IsActive
            })
            .ToList();
    }
}
