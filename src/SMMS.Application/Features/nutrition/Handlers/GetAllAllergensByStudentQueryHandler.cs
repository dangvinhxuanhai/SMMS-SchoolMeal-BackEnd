using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediatR;
using SMMS.Application.Features.nutrition.Commands;
using SMMS.Application.Features.nutrition.DTOs;
using SMMS.Application.Features.nutrition.Interfaces;
using SMMS.Application.Features.nutrition.Queries;

namespace SMMS.Application.Features.nutrition.Handlers
{
    public class GetAllAllergensByStudentQueryHandler
        : IRequestHandler<GetAllAllergensByStudentQuery, List<AllergenDTO>>,
        IRequestHandler<GetTopAllergensQuery, List<AllergenDTO>>
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
        public async Task<List<AllergenDTO>> Handle(GetTopAllergensQuery request, CancellationToken cancellationToken)
        {
            return await _allergenRepository.GetTopAsync(request.StudentId, request.Top);
        }
    }
    public class AddStudentAllergyHandler
       : IRequestHandler<AddStudentAllergyCommand>
    {
        private readonly IAllergenRepository _allergenRepository;

        public AddStudentAllergyHandler(IAllergenRepository allergenRepository)
        {
            _allergenRepository = allergenRepository;
        }

        public async Task Handle(
     AddStudentAllergyCommand request,
     CancellationToken cancellationToken)
        {
            await _allergenRepository.AddStudentAllergyAsync(
                request.UserId,
                request.StudentId,
                new AddStudentAllergyDTO
                {
                    IngredientId = request.IngredientId,
                    Notes = request.Notes,
                    ReactionNotes = request.ReactionNotes,
                    HandlingNotes = request.HandlingNotes,
                    SeverityLevel = request.SeverityLevel
                });
        }
    }
}

