using SMMS.Application.Features.school.DTOs;

namespace SMMS.Application.Features.school.Interfaces
{
    public interface IStudentHealthService
    {
        Task<StudentBMIResultDto?> GetCurrentBMIAsync(Guid studentId, Guid parentId);
        Task<IEnumerable<StudentBMIResultDto>> GetBMIByYearsAsync(Guid studentId, Guid parentId, string? yearFilter = null);
    }
}