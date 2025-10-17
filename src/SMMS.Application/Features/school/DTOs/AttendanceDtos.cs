using System;
using System.Collections.Generic;

namespace SMMS.Application.Features.school.DTOs
{
    public class AttendanceHistoryDto
    {
        public List<AttendanceResponseDto> Records { get; set; } = new List<AttendanceResponseDto>();
        public int TotalCount { get; set; }
    }

    public class AttendanceResponseDto
    {
        public int AttendanceId { get; set; }
        public Guid StudentId { get; set; }
        public string StudentName { get; set; }
        public DateOnly AbsentDate { get; set; }
        public string Reason { get; set; }
        public string NotifiedBy { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class AttendanceParentDto
    {
        public Guid StudentId { get; set; }
        public string StudentName { get; set; }
        public DateOnly AbsentDate { get; set; }
        public string Reason { get; set; }
        public string NotifiedBy { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class AttendanceRequestDto
    {
        public Guid StudentId { get; set; }
        public DateOnly StartDate { get; set; }
        public DateOnly EndDate { get; set; }
        public string Reason { get; set; }
    }
}
