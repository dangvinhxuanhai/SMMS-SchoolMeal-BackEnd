namespace SMMS.WebAPI.Controllers.Modules.KitchenStaff.Fastapi.DTOs;

public class CreateAcademicYearDto
{
    public string YearName { get; set; } = null!;
    public DateTime? BoardingStartDate { get; set; }
    public DateTime? BoardingEndDate { get; set; }
    public Guid? SchoolId { get; set; }
}

