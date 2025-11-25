using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediatR;
using SMMS.Application.Features.Manager.DTOs;

namespace SMMS.Application.Features.Manager.Commands;
public record CreateManagerNotificationCommand(
    CreateManagerNotificationRequest Request,
    Guid SenderId,
    Guid SchoolId
) : IRequest<ManagerNotificationDto>;

public record UpdateManagerNotificationCommand(
    long NotificationId,
    UpdateManagerNotificationRequest Request,
    Guid SenderId,
    Guid SchoolId
) : IRequest<ManagerNotificationDto>;

public record DeleteManagerNotificationCommand(
    long NotificationId,
    Guid SenderId
) : IRequest<bool>;
