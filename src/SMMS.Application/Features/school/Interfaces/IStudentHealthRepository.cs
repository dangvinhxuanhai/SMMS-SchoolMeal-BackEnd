using SMMS.Application.Features.school.DTOs;

namespace SMMS.Application.Features.school.Interfaces
{
    public interface IStudentHealthRepository
    {
        Task<StudentBMIResultDto?> GetCurrentBMIAsync(Guid studentId, Guid parentId);
        Task<IEnumerable<StudentBMIResultDto>> GetBMIByYearsAsync(Guid studentId, Guid parentId, string? yearFilter = null);
        /// <summary>
        /// Lấy BMI trung bình của **lớp đầu tiên** trong 1 trường.
        /// Nếu không đủ dữ liệu (không có lớp, không có HS, không có health record)
        /// thì trả về null.
        /// </summary>
        Task<double?> GetAverageBmiForFirstClassAsync(
            Guid schoolId,
            CancellationToken cancellationToken = default);
    }
}
