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

        public int IngredientId { get; set; }

        public int? SeverityLevel { get; set; }
        public string? Notes { get; set; }
        public string? ReactionNotes { get; set; }
        public string? HandlingNotes { get; set; }
    }
}
