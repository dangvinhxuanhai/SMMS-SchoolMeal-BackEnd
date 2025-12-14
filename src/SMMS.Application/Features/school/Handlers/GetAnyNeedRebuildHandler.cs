using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediatR;
using SMMS.Application.Features.school.Interfaces;
using SMMS.Application.Features.school.Queries;

namespace SMMS.Application.Features.school.Handlers;
public class GetAnyNeedRebuildHandler : IRequestHandler<GetAnyNeedRebuildQuery, bool>
{
    private readonly ISchoolRepository _schoolRepository;

    public GetAnyNeedRebuildHandler(ISchoolRepository schoolRepository)
    {
        _schoolRepository = schoolRepository;
    }

    // schools có NeedRebuildAiIndex = false nghia la truong do can dc build lai AI index
    public async Task<bool> Handle(GetAnyNeedRebuildQuery request, CancellationToken cancellationToken)
    {
        // Trả về true nếu có ít nhất 1 school có NeedRebuildAiIndex = false (0)
        return await _schoolRepository.AnyNeedRebuildAsync(cancellationToken);
    }
}
