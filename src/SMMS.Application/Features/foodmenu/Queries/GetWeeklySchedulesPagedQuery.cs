using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediatR;
using SMMS.Application.Features.foodmenu.DTOs;

namespace SMMS.Application.Features.foodmenu.Queries;
public sealed record GetWeeklySchedulesPagedQuery(
    Guid SchoolId,
    int PageIndex = 1,
    int PageSize = 10,
    bool GetAll = false
) : IRequest<PagedResult<WeeklyScheduleDto>>;
