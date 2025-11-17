namespace SMMS.WebAPI.Controllers.Modules.KitchenStaff.Fastapi.DTOs;

public class CreateSchoolDto
{
    public string SchoolName { get; set; } = null!;
    public string? ContactEmail { get; set; }
    public string? Hotline { get; set; }
    public string? SchoolContract { get; set; }
    public string? SchoolAddress { get; set; }
    public bool IsActive { get; set; } = true;
    public Guid? CreatedBy { get; set; }
}

