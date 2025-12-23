using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SMMS.Application.Features.nutrition.DTOs
{
    public class AllergenDTO
    {
        public int AllergenId { get; set; }
        public string AllergenName { get; set; } = null!;
        public string? AllergenMatter { get; set; }
        public string? AllergenInfo { get; set; }
        public DateTime CreatedAt { get; set; }

        public string? ReactionNotes { get; set; }
        public string? Notes { get; set; }
    }

    public class AddStudentAllergyDTO
    {
        public int IngredientId { get; set; }

        public string? Notes { get; set; }
        public string? ReactionNotes { get; set; }
        public string? HandlingNotes { get; set; }
        public int? SeverityLevel { get; set; }
    }
}
