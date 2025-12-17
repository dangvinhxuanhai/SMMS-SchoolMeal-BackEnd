using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using MediatR;
using SMMS.Application.Features.nutrition.DTOs;

namespace SMMS.Application.Features.nutrition.Commands
{
    public class AddStudentAllergyCommand : IRequest
    {
        public Guid UserId { get; set; }
        public Guid StudentId { get; set; }

        public int? AllergenId { get; set; }
        public string? AllergenName { get; set; }

        public string? AllergenInfo { get; set; }
    }
}
