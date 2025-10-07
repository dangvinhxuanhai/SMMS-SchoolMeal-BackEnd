using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace SMMS.Domain.Entities;

[Index("AllergenName", Name = "UQ__Allergen__7D9886191633E3D1", IsUnique = true)]
public partial class Allergen
{
    [Key]
    public int AllergenId { get; set; }

    [StringLength(100)]
    public string AllergenName { get; set; } = null!;

    [StringLength(500)]
    public string? AllergenMatter { get; set; }

    [StringLength(300)]
    public string? AllergenInfo { get; set; }

    [InverseProperty("Allergen")]
    public virtual ICollection<AllergeticIngredient> AllergeticIngredients { get; set; } = new List<AllergeticIngredient>();

    [InverseProperty("Allergen")]
    public virtual ICollection<StudentAllergen> StudentAllergens { get; set; } = new List<StudentAllergen>();
}
