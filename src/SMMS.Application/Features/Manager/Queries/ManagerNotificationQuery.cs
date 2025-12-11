using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediatR;
using SMMS.Application.Features.billing.DTOs;
using SMMS.Application.Features.Manager.DTOs;

namespace SMMS.Application.Features.Manager.Queries;
public record GetManagerNotificationByIdQuery(long NotificationId)
    : IRequest<ManagerNotificationDto?>;

public record GetManagerNotificationsBySenderQuery(
    Guid SenderId,
    int Page = 1,
    int PageSize = 20
) : IRequest<PagedResult<ManagerNotificationDto>>;
