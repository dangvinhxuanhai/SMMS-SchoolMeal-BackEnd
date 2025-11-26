using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediatR;
using SMMS.Application.Features.school.DTOs;
using SMMS.Application.Features.school.Interfaces;
using SMMS.Application.Features.school.Queries;

namespace SMMS.Application.Features.school.Handlers
{
    public class StudentHealthQueryHandler :
          IRequestHandler<GetCurrentBMIQuery, StudentBMIResultDto?>,
          IRequestHandler<GetBMIHistoryQuery, IEnumerable<StudentBMIResultDto>>
    {
        private readonly IStudentHealthRepository _studentHealthRepository;

        public StudentHealthQueryHandler(IStudentHealthRepository studentHealthRepository)
        {
            _studentHealthRepository = studentHealthRepository;
        }

        // ✅ Handler cho GetCurrentBMIQuery
        public async Task<StudentBMIResultDto?> Handle(GetCurrentBMIQuery request, CancellationToken cancellationToken)
        {
            return await _studentHealthRepository.GetCurrentBMIAsync(request.StudentId, request.ParentId);
        }

        // ✅ Handler cho GetBMIHistoryQuery
        public async Task<IEnumerable<StudentBMIResultDto>> Handle(GetBMIHistoryQuery request, CancellationToken cancellationToken)
        {
            return await _studentHealthRepository.GetBMIByYearsAsync(request.StudentId, request.ParentId, request.Year);
        }
    }
}
