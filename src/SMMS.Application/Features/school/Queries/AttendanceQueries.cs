using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediatR;
using SMMS.Application.Features.school.DTOs;

namespace SMMS.Application.Features.school.Queries
{
    public class GetAttendanceByStudentQuery : IRequest<AttendanceHistoryDto>
    {
        public Guid StudentId { get; set; }

        public GetAttendanceByStudentQuery(Guid studentId)
        {
            StudentId = studentId;
        }
    }
    public class GetAttendanceByParentQuery : IRequest<AttendanceHistoryDto>
    {
        public Guid ParentId { get; set; }

        public GetAttendanceByParentQuery(Guid parentId)
        {
            ParentId = parentId;
        }
    }
}
