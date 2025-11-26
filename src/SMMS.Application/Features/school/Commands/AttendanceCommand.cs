using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediatR;
using SMMS.Application.Features.school.DTOs;

namespace SMMS.Application.Features.school.Commands
{
    public class CreateAttendanceCommand : IRequest<bool>
    {
        public AttendanceRequestDto Request { get; set; }
        public Guid ParentId { get; set; }

        public CreateAttendanceCommand(AttendanceRequestDto request, Guid parentId)
        {
            Request = request;
            ParentId = parentId;
        }
    }
}
