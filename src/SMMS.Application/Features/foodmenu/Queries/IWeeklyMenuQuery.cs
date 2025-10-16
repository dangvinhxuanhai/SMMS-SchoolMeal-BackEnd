using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediatR;
using SMMS.Application.Features.foodmenu.DTOs;

namespace SMMS.Application.Features.foodmenu.Queries;

public record GetWeekMenuQuery(Guid StudentId, DateTime AnyDateInWeek) : IRequest<WeekMenuDto?>;
public record GetAvailableWeeksQuery(Guid StudentId, DateTime? From = null, DateTime? To = null)
        : IRequest<IReadOnlyList<WeekOptionDto>>;
