using System;
using System.Collections.Generic;

namespace SMMS.Application.Features.school.DTOs
{
    public class CreateAttendanceRequestDto
    {
        public Guid StudentId { get; set; }
        public DateOnly AbsentDate { get; set; }
        public string Reason { get; set; }
    }

    public class AttendanceResponseDto
    {
        public int AttendanceId { get; set; }
        public Guid StudentId { get; set; }
        public string StudentName { get; set; }
        public DateOnly AbsentDate { get; set; }
        public string Reason { get; set; }
        public string NotifiedBy { get; set; } // Tên người thông báo
        public DateTime CreatedAt { get; set; }
    }

    public class AttendanceHistoryDto
    {
        public List<AttendanceResponseDto> Records { get; set; } = new List<AttendanceResponseDto>();
        public int TotalCount { get; set; }
    }
}
