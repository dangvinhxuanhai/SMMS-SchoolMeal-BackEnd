using System.Threading.Tasks;
using System.Collections.Generic;
using SMMS.Application.Features.Wardens.DTOs;

namespace SMMS.Application.Features.Wardens.Interfaces;
    public interface IWardensHealthRepository
    {
    /// <summary>
    /// Tổng quan sức khỏe (BMI) của tất cả học sinh trong các lớp mà giáo viên phụ trách.
    /// </summary>
    Task<HealthSummaryDto> GetHealthSummaryAsync(
        Guid wardenId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Danh sách BMI (lần đo gần nhất) của học sinh trong 1 lớp.
    /// </summary>
    Task<IEnumerable<StudentHealthDto>> GetStudentsHealthAsync(
        Guid classId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Lịch sử tất cả lần đo BMI của 1 học sinh.
    /// </summary>
    Task<IEnumerable<StudentHealthDto>> GetStudentBmiHistoryAsync(
        Guid studentId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Tạo mới 1 record BMI cho học sinh.
    /// </summary>
    Task<StudentHealthDto> CreateStudentBmiAsync(
        Guid studentId,
        double heightCm,
        double weightKg,
        DateTime recordDate,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Cập nhật 1 record BMI theo RecordId.
    /// </summary>
    Task<StudentHealthDto?> UpdateStudentBmiAsync(
        Guid recordId,
        double heightCm,
        double weightKg,
        DateTime recordDate,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Xoá 1 record BMI theo RecordId.
    /// </summary>
    Task<bool> DeleteStudentBmiAsync(
        Guid recordId,
        CancellationToken cancellationToken = default);
}



