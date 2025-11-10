using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediatR;
using SMMS.Application.Features.school.Commands;
using SMMS.Application.Features.school.DTOs;
using SMMS.Application.Features.school.Interfaces;
using SMMS.Application.Features.school.Queries;
using SMMS.Domain.Entities.school;

namespace SMMS.Application.Features.school.Handlers
{
    public class AttendanceCommandHandler :
       IRequestHandler<CreateAttendanceCommand, bool>,
       IRequestHandler<GetAttendanceByStudentQuery, AttendanceHistoryDto>,
       IRequestHandler<GetAttendanceByParentQuery, AttendanceHistoryDto>
    {
        private readonly IAttendanceRepository _attendanceRepository;

        public AttendanceCommandHandler(IAttendanceRepository attendanceRepository)
        {
            _attendanceRepository = attendanceRepository;
        }

        // =========================================
        // 🟢 Handle: Tạo đơn xin nghỉ học
        // =========================================
        public async Task<bool> Handle(CreateAttendanceCommand request, CancellationToken cancellationToken)
        {
            return await _attendanceRepository.CreateAttendanceAsync(request.Request, request.ParentId);
        }

        // =========================================
        // 🔵 Handle: Lấy lịch sử điểm danh theo học sinh
        // =========================================
        public async Task<AttendanceHistoryDto> Handle(GetAttendanceByStudentQuery request, CancellationToken cancellationToken)
        {
            return await _attendanceRepository.GetAttendanceHistoryByStudentAsync(request.StudentId);
        }

        // =========================================
        // 🟣 Handle: Lấy lịch sử điểm danh theo phụ huynh
        // =========================================
        public async Task<AttendanceHistoryDto> Handle(GetAttendanceByParentQuery request, CancellationToken cancellationToken)
        {
            return await _attendanceRepository.GetAttendanceHistoryByParentAsync(request.ParentId);
        }
    }
}
