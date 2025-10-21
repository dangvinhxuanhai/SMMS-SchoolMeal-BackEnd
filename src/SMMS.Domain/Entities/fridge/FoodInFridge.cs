using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using SMMS.Domain.Entities.auth;
using SMMS.Domain.Entities.foodmenu;
using SMMS.Domain.Entities.nutrition;
using SMMS.Domain.Entities.school;

namespace SMMS.Domain.Entities.fridge;

[Table("FoodInFridge", Schema = "fridge")]
public partial class FoodInFridge
{
    [Key]
    public long SampleId { get; set; }

    public Guid SchoolId { get; set; }

    public int? YearId { get; set; }

    public int? FoodId { get; set; }

    public int? MenuId { get; set; }

    public Guid StoredBy { get; set; }

    public DateTime StoredAt { get; set; }

    [Column(TypeName = "decimal(5, 2)")]
    public decimal? TemperatureC { get; set; }

    public bool IsDeleted { get; set; }

    public DateTime? DeletedAt { get; set; }

    public Guid? DeletedBy { get; set; }

    [StringLength(500)]
    public string? Note { get; set; }

    [ForeignKey("DeletedBy")]
    [InverseProperty("FoodInFridgeDeletedByNavigations")]
    public virtual User? DeletedByNavigation { get; set; }

    [ForeignKey("FoodId")]
    [InverseProperty("FoodInFridges")]
    public virtual FoodItem? Food { get; set; }

    [ForeignKey("MenuId")]
    [InverseProperty("FoodInFridges")]
    public virtual Menu? Menu { get; set; }

    [ForeignKey("SchoolId")]
    [InverseProperty("FoodInFridges")]
    public virtual School School { get; set; } = null!;

    [ForeignKey("StoredBy")]
    [InverseProperty("FoodInFridgeStoredByNavigations")]
    public virtual User StoredByNavigation { get; set; } = null!;

    [ForeignKey("YearId")]
    [InverseProperty("FoodInFridges")]
    public virtual AcademicYear? Year { get; set; }

    [ForeignKey("SampleId")]
    [InverseProperty("Samples")]
    public virtual ICollection<Ingredient> Ingredients { get; set; } = new List<Ingredient>();
}
