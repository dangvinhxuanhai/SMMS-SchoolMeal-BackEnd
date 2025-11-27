using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediatR;
using SMMS.Application.Features.billing.DTOs;

namespace SMMS.Application.Features.billing.Commands
{
    public class CreateNotificationCommand : IRequest<AdminNotificationDto>
    {
        public CreateNotificationDto Dto { get; set; }
        public Guid AdminId { get; set; }

        public CreateNotificationCommand(CreateNotificationDto dto, Guid adminId)
        {
            Dto = dto;
            AdminId = adminId;
        }
    }
}
