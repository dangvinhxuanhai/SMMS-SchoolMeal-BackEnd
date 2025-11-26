using SMMS.Application.Features.school.DTOs;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SMMS.Application.Features.school.Interfaces
{
    public interface IAttendanceService
    {
        Task<bool> CreateAttendanceAsync(AttendanceRequestDto request, Guid parentId);
        Task<AttendanceHistoryDto> GetAttendanceHistoryByStudentAsync(Guid studentId);
        Task<AttendanceHistoryDto> GetAttendanceHistoryByParentAsync(Guid parentId); // Lấy lịch sử nghỉ của tất cả con của parent
    }
}