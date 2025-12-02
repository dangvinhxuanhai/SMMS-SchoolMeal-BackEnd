namespace SMMS.Application.Features.Wardens.DTOs;

public class ClassDto
{
    public Guid ClassId { get; set; }
    public string ClassName { get; set; } = string.Empty;
    public string SchoolName { get; set; } = string.Empty;
    public Guid WardenId { get; set; }
    public string WardenName { get; set; } = string.Empty;
    public int TotalStudents { get; set; }
    public int PresentToday { get; set; }
    public int AbsentToday { get; set; }
    public double AttendanceRate { get; set; }
}

public class ClassAttendanceDto
{
    public Guid ClassId { get; set; }
    public string ClassName { get; set; } = string.Empty;
    public List<StudentAttendanceDto> Students { get; set; } = new();
    public AttendanceSummaryDto Summary { get; set; } = new();
}

public class StudentAttendanceDto
{
    public Guid StudentId { get; set; }
    public string StudentName { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string? Reason { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class AttendanceSummaryDto
{
    public int TotalStudents { get; set; }
    public int Present { get; set; }
    public int Absent { get; set; }
    public int Late { get; set; }
    public double AttendanceRate { get; set; }
}

public class StudentDto
{
    public Guid StudentId { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string? Gender { get; set; }
    public DateOnly? DateOfBirth { get; set; }
    public string? AvatarUrl { get; set; }
    public bool IsActive { get; set; }
    public string? ParentName { get; set; }
    public string? ParentPhone { get; set; }
    public string? RelationName { get; set; }
    public List<string> Allergies { get; set; } = new List<string>();
    public bool IsAbsent { get; set; }
}

public class HealthSummaryDto
{
    public int TotalStudents { get; set; }
    public int NormalWeight { get; set; }
    public int Underweight { get; set; }
    public int Overweight { get; set; }
    public int Obese { get; set; }
    public double AverageBMI { get; set; }
}

public class StudentHealthDto
{
    public Guid StudentId { get; set; }
    public string StudentName { get; set; } = string.Empty;
    public double? HeightCm { get; set; }
    public double? WeightKg { get; set; }
    public double? BMI { get; set; }
    public string? BMICategory { get; set; }
    public DateTime RecordDate { get; set; }
    public Guid? RecordId { get; set; }
}

public class DashboardDto
{
    public int TotalClasses { get; set; }
    public int TotalStudents { get; set; }
    public int PresentToday { get; set; }
    public int AbsentToday { get; set; }
    public double AttendanceRate { get; set; }
    public List<RecentActivityDto> RecentActivities { get; set; } = new();
}

public class RecentActivityDto
{
    public string Activity { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
    public string Type { get; set; } = string.Empty;
}

public class NotificationDto
{
    public long NotificationId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public bool IsRead { get; set; }
    public string SendType { get; set; } = string.Empty;
}
public class CreateBmiRequest
{
    public double HeightCm { get; set; }
    public double WeightKg { get; set; }
    public DateTime RecordDate { get; set; }
}

public class UpdateBmiRequest
{
    public double HeightCm { get; set; }
    public double WeightKg { get; set; }
    public DateTime RecordDate { get; set; }
}
