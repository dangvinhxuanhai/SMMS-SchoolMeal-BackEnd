namespace SMMS.WebAPI.Controllers.Modules.KitchenStaff.Fastapi.DTOs;

public class CreateUserDto
{
    public string Email { get; set; } = null!;
    public string PasswordHash { get; set; } = null!;
    public string FullName { get; set; } = null!;
    public string Phone { get; set; } = null!;
    public string? LanguagePref { get; set; } = "vi";
    public int RoleId { get; set; }
    public Guid? SchoolId { get; set; }
    public bool IsActive { get; set; } = true;
    public Guid? CreatedBy { get; set; }
    public string? IdentityNo { get; set; }
    public DateOnly? DateOfBirth { get; set; }
}
