using Microsoft.EntityFrameworkCore;
using SMMS.Application.Features.school.DTOs;
using SMMS.Application.Features.school.Interfaces;
using SMMS.Domain.Models.school;
using SMMS.Persistence.DbContextSite;

namespace SMMS.Infrastructure.Service
{
    public class AttendanceService : IAttendanceService
    {
        private readonly EduMealContext _dbContext;

        public AttendanceService(EduMealContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<bool> CreateAttendanceAsync(CreateAttendanceRequestDto request, Guid notifiedByUserId)
        {
            var student = await _dbContext.Students
                .FirstOrDefaultAsync(s => s.StudentId == request.StudentId);

            if (student == null)
                throw new Exception("Học sinh không tồn tại.");

            // Sửa: So sánh DateOnly với DateOnly (không cần .Date)
            var existingAttendance = await _dbContext.Attendances
                .FirstOrDefaultAsync(a => a.StudentId == request.StudentId && a.AbsentDate == request.AbsentDate);

            if (existingAttendance != null)
                throw new Exception("Đã có đơn xin nghỉ cho ngày này.");

            var attendance = new Attendance
            {
                StudentId = request.StudentId,
                AbsentDate = request.AbsentDate, // Sửa: Gán trực tiếp DateOnly
                Reason = request.Reason,
                NotifiedBy = notifiedByUserId,
                CreatedAt = DateTime.UtcNow
            };

            await _dbContext.Attendances.AddAsync(attendance);
            await _dbContext.SaveChangesAsync();

            return true;
        }

        public async Task<AttendanceHistoryDto> GetAttendanceHistoryByStudentAsync(Guid studentId)
        {
            var records = await _dbContext.Attendances
                .Where(a => a.StudentId == studentId)
                .OrderByDescending(a => a.AbsentDate)
                .Select(a => new AttendanceResponseDto
                {
                    AttendanceId = a.AttendanceId,
                    StudentId = a.StudentId,
                    StudentName = a.Student.FullName,
                    AbsentDate = a.AbsentDate, // Sửa: Gán trực tiếp DateOnly
                    Reason = a.Reason,
                    NotifiedBy = a.NotifiedByNavigation != null ? a.NotifiedByNavigation.FullName : null,
                    CreatedAt = a.CreatedAt
                })
                .ToListAsync();

            return new AttendanceHistoryDto
            {
                Records = records,
                TotalCount = records.Count // Sửa: records.Count (không có dấu ngoặc)
            };
        }

        public async Task<AttendanceHistoryDto> GetAttendanceHistoryByParentAsync(Guid parentId)
        {
            var studentIds = await _dbContext.Students
                .Where(s => s.ParentId == parentId)
                .Select(s => s.StudentId)
                .ToListAsync();

            var records = await _dbContext.Attendances
                .Where(a => studentIds.Contains(a.StudentId))
                .OrderByDescending(a => a.AbsentDate)
                .Select(a => new AttendanceResponseDto
                {
                    AttendanceId = a.AttendanceId,
                    StudentId = a.StudentId,
                    StudentName = a.Student.FullName,
                    AbsentDate = a.AbsentDate, // Sửa: Gán trực tiếp DateOnly
                    Reason = a.Reason,
                    NotifiedBy = a.NotifiedByNavigation != null ? a.NotifiedByNavigation.FullName : null,
                    CreatedAt = a.CreatedAt
                })
                .ToListAsync();

            return new AttendanceHistoryDto
            {
                Records = records,
                TotalCount = records.Count // Sửa: records.Count (không có dấu ngoặc)
            };
        }
    }
}
