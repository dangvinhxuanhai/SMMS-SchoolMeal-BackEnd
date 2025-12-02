namespace SMMS.Application.Features.Manager.DTOs;

public class AcademicYearDto
{
    public int YearId { get; set; }
    public string YearName { get; set; } = string.Empty;
    public DateTime? BoardingStartDate { get; set; }
    public DateTime? BoardingEndDate { get; set; }
    public Guid? SchoolId { get; set; }
}
public class CreateAcademicYearRequest
{
    public string YearName { get; set; } = string.Empty;
    public DateTime? BoardingStartDate { get; set; }
    public DateTime? BoardingEndDate { get; set; }

    // sẽ được set từ token ở Controller
    public Guid SchoolId { get; set; }
}

public class UpdateAcademicYearRequest
{
    public string? YearName { get; set; }
    public DateTime? BoardingStartDate { get; set; }
    public DateTime? BoardingEndDate { get; set; }
}
