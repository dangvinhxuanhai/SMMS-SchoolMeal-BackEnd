using SMMS.Application.Features.Wardens.DTOs;

namespace SMMS.Application.Features.Wardens.Interfaces;

public interface IWardensService
{
    Task<IEnumerable<ClassDto>> GetClassesAsync(Guid wardenId);
    Task<ClassAttendanceDto> GetClassAttendanceAsync(Guid classId);
    Task<byte[]> ExportAttendanceReportAsync(Guid classId);
    Task<byte[]> ExportClassStudentsAsync(Guid classId);
    Task<byte[]> ExportClassHealthAsync(Guid classId);
    Task<IEnumerable<StudentDto>> GetStudentsInClassAsync(Guid classId);
    Task<HealthSummaryDto> GetHealthSummaryAsync(Guid wardenId);
    Task<IEnumerable<StudentHealthDto>> GetStudentsHealthAsync(Guid classId);
    Task<object> GetHealthRecordsAsync(Guid classId);
    Task<DashboardDto> GetDashboardAsync(Guid wardenId);
    Task<IEnumerable<NotificationDto>> GetNotificationsAsync(Guid wardenId);
    Task<object> SearchAsync(Guid classId, string keyword);
}
