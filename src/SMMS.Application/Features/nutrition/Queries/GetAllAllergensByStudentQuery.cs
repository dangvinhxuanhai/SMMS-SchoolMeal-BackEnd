using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediatR;
using SMMS.Application.Features.nutrition.DTOs;

namespace SMMS.Application.Features.nutrition.Queries
{
    public class GetAllAllergensByStudentQuery : IRequest<List<AllergenDTO>>
    {
        public Guid StudentId { get; }

        public GetAllAllergensByStudentQuery(Guid studentId)
        {
            StudentId = studentId;
        }
    }
    public class GetTopAllergensQuery : IRequest<List<AllergenDTO>>
    {
        public Guid StudentId { get; set; }
        public int Top { get; set; } = 5;

        public GetTopAllergensQuery(Guid studentId, int top = 5)
        {
            StudentId = studentId;
            Top = top;
        }
    }
}
