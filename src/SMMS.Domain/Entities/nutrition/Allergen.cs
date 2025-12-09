using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using SMMS.Domain.Entities.auth;
using SMMS.Domain.Entities.school;

namespace SMMS.Domain.Entities.nutrition;

[Table("Allergens", Schema = "nutrition")]
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

    [ForeignKey("SchoolId")]
    [InverseProperty("Allergens")]
    public virtual School School { get; set; } = null!;

    [InverseProperty("Allergen")]
    public virtual ICollection<StudentAllergen> StudentAllergens { get; set; } = new List<StudentAllergen>();
}
