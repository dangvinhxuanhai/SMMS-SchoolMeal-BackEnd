using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using SMMS.Domain.Models.foodmenu;
using SMMS.Domain.Models.fridge;

namespace SMMS.Domain.Models.school;

[Table("AcademicYears", Schema = "school")]
[Index("YearName", Name = "UQ__Academic__294C4DA9C2B87D92", IsUnique = true)]
public partial class AcademicYear
{
    [Key]
    public int YearId { get; set; }

    [StringLength(20)]
    public string YearName { get; set; } = null!;

    public DateTime? BoardingStartDate { get; set; }

    public DateTime? BoardingEndDate { get; set; }

    public Guid? SchoolId { get; set; }

    [InverseProperty("Year")]
    public virtual ICollection<Class> Classes { get; set; } = new List<Class>();

    [InverseProperty("Year")]
    public virtual ICollection<FoodInFridge> FoodInFridges { get; set; } = new List<FoodInFridge>();

    [InverseProperty("Year")]
    public virtual ICollection<Menu> Menus { get; set; } = new List<Menu>();

    [ForeignKey("SchoolId")]
    [InverseProperty("AcademicYears")]
    public virtual School? School { get; set; }

    [InverseProperty("Year")]
    public virtual ICollection<StudentHealthRecord> StudentHealthRecords { get; set; } = new List<StudentHealthRecord>();

    [InverseProperty("Year")]
    public virtual ICollection<StudentImage> StudentImages { get; set; } = new List<StudentImage>();
}
