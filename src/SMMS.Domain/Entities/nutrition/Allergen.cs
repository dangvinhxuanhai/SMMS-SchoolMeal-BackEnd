using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using SMMS.Domain.Models.auth;
using SMMS.Domain.Models.rag;
using SMMS.Domain.Models.school;

namespace SMMS.Domain.Models.nutrition;

[Table("Allergens", Schema = "nutrition")]
[Index("AllergenName", Name = "UQ__Allergen__7D988619907EC52D", IsUnique = true)]
public partial class Allergen
{
    [Key]
    public int AllergenId { get; set; }

    [StringLength(100)]
    public string AllergenName { get; set; } = null!;

    [StringLength(500)]
    public string? AllergenMatter { get; set; }

    public Guid SchoolId { get; set; }

    public Guid? CreatedBy { get; set; }

    public DateTime CreatedAt { get; set; }

    [StringLength(300)]
    public string? AllergenInfo { get; set; }

    [InverseProperty("Allergen")]
    public virtual ICollection<AllergeticIngredient> AllergeticIngredients { get; set; } = new List<AllergeticIngredient>();

    [ForeignKey("CreatedBy")]
    [InverseProperty("Allergens")]
    public virtual User? CreatedByNavigation { get; set; }

    [InverseProperty("Allergen")]
    public virtual ICollection<RagRequestAllergen> RagRequestAllergens { get; set; } = new List<RagRequestAllergen>();

    [ForeignKey("SchoolId")]
    [InverseProperty("Allergens")]
    public virtual School School { get; set; } = null!;

    [InverseProperty("Allergen")]
    public virtual ICollection<StudentAllergen> StudentAllergens { get; set; } = new List<StudentAllergen>();
}
