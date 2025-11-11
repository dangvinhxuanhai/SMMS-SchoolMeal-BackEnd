using Microsoft.EntityFrameworkCore;
using SMMS.Application.Features.school.DTOs;
using SMMS.Application.Features.school.Interfaces;
using SMMS.Domain.Entities.school;
using SMMS.Persistence.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SMMS.Persistence.Repositories.schools
{
    public class AttendanceRepository : IAttendanceRepository
    {
        private readonly EduMealContext _context;

        public AttendanceRepository(EduMealContext context)
        {
            _context = context;
        }

        public async Task<bool> CreateAttendanceAsync(AttendanceRequestDto request, Guid parentId)
        {
            var student = await _context.Students
                .FirstOrDefaultAsync(s => s.StudentId == request.StudentId);

            if (student == null)
                throw new Exception("Học sinh không tồn tại.");

            if (student.ParentId != parentId)
                throw new Exception("Bạn không có quyền gửi đơn cho học sinh này.");

            var days = Enumerable.Range(0, request.EndDate.DayNumber - request.StartDate.DayNumber + 1)
                                 .Select(offset => request.StartDate.AddDays(offset));

            foreach (var day in days)
            {
                var exists = await _context.Attendances
                    .AnyAsync(a => a.StudentId == request.StudentId && a.AbsentDate == day);
                if (exists) continue;

                var attendance = new Attendance
                {
                    StudentId = request.StudentId,
                    AbsentDate = day,
                    Reason = request.Reason,
                    NotifiedBy = parentId,
                    CreatedAt = DateTime.UtcNow
                };

                _context.Attendances.Add(attendance);
            }

            await _context.SaveChangesAsync();

            return true; // ✅ trả về bool
        }



        public async Task<AttendanceHistoryDto> GetAttendanceHistoryByStudentAsync(Guid studentId)
        {
            var records = await _context.Attendances
                .Where(a => a.StudentId == studentId)
                .OrderByDescending(a => a.AbsentDate)
                .Select(a => new AttendanceResponseDto
                {
                    AttendanceId = a.AttendanceId,
                    StudentId = a.StudentId,
                    StudentName = a.Student.FullName,
                    AbsentDate = a.AbsentDate,
                    Reason = a.Reason,
                    NotifiedBy = a.NotifiedByNavigation != null ? a.NotifiedByNavigation.FullName : null,
                    CreatedAt = a.CreatedAt
                })
                .ToListAsync();

            return new AttendanceHistoryDto
            {
                Records = records,
                TotalCount = records.Count
            };
        }


        public async Task<AttendanceHistoryDto> GetAttendanceHistoryByParentAsync(Guid parentId)
        {
            var studentIds = await _context.Students
                .Where(s => s.ParentId == parentId)
                .Select(s => s.StudentId)
                .ToListAsync();

            var records = await _context.Attendances
                .Where(a => studentIds.Contains(a.StudentId))
                .OrderByDescending(a => a.AbsentDate)
                .Select(a => new AttendanceResponseDto
                {
                    AttendanceId = a.AttendanceId,
                    StudentId = a.StudentId,
                    StudentName = a.Student.FullName,
                    AbsentDate = a.AbsentDate,
                    Reason = a.Reason,
                    NotifiedBy = a.NotifiedByNavigation != null ? a.NotifiedByNavigation.FullName : null,
                    CreatedAt = a.CreatedAt
                })
                .ToListAsync();

            return new AttendanceHistoryDto
            {
                Records = records,
                TotalCount = records.Count
            };
        }

    }
}
