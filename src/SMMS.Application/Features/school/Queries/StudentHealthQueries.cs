using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediatR;
using SMMS.Application.Features.school.DTOs;

namespace SMMS.Application.Features.school.Queries
{
    public record GetCurrentBMIQuery(Guid StudentId, Guid ParentId) : IRequest<StudentBMIResultDto?>;

    // ✅ Lấy lịch sử BMI (có thể lọc theo năm)
    public record GetBMIHistoryQuery(Guid StudentId, Guid ParentId, string? Year)
        : IRequest<IEnumerable<StudentBMIResultDto>>;
}
