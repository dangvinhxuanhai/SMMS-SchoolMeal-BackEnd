using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediatR;
using SMMS.Application.Features.nutrition.DTOs;
using SMMS.Application.Features.nutrition.Interfaces;
using SMMS.Application.Features.nutrition.Queries;

namespace SMMS.Application.Features.nutrition.Handlers
{
    public class GetAllAllergensByStudentQueryHandler
        : IRequestHandler<GetAllAllergensByStudentQuery, List<AllergenDTO>>
    {
        private readonly IAllergenRepository _allergenRepository;

        public GetAllAllergensByStudentQueryHandler(IAllergenRepository allergenRepository)
        {
            _allergenRepository = allergenRepository;
        }

        public async Task<List<AllergenDTO>> Handle(
            GetAllAllergensByStudentQuery request,
            CancellationToken cancellationToken)
        {
            if (request.StudentId == Guid.Empty)
                throw new ArgumentException("StudentId không hợp lệ");

            return await _allergenRepository.GetAllAsync(request.StudentId);
        }
    }
}
