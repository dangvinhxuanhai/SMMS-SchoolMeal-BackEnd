namespace SMMS.WebAPI.DTOs;

public class ClassAttendanceDto
{
    public Guid ClassId { get; set; }
    public string ClassName { get; set; } = null!;
    public string SchoolName { get; set; } = null!;
    public int TotalStudents { get; set; }
    public int PresentToday { get; set; }
    public int AbsentToday { get; set; }
}
