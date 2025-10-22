using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediatR;
using Microsoft.Extensions.Logging;
using SMMS.Application.Abstractions;
using SMMS.Application.Common.Exceptions;
using SMMS.Application.Features.foodmenu.Commands;
using SMMS.Application.Features.foodmenu.DTOs;
using SMMS.Application.Features.foodmenu.Interfaces;
using SMMS.Application.Features.foodmenu.Queries;
using SMMS.Domain.Entities.foodmenu;

namespace SMMS.Application.Features.foodmenu.Handlers;
public sealed class WeeklyMenuHandler :
    IRequestHandler<GetWeekMenuQuery, WeekMenuDto?>,
    IRequestHandler<GetAvailableWeeksQuery, IReadOnlyList<WeekOptionDto>>
{
    private readonly IWeeklyMenuRepository _repo;
    private readonly ILogger<WeeklyMenuHandler> _logger;

    public WeeklyMenuHandler(IWeeklyMenuRepository repo, ILogger<WeeklyMenuHandler> logger)
    {
        _repo = repo;
        _logger = logger;
    }

    public async Task<WeekMenuDto?> Handle(GetWeekMenuQuery request, CancellationToken ct)
    {
        if (request.StudentId == Guid.Empty) return null;
        if (request.AnyDateInWeek == default) return null;

        try
        {
            return await _repo.GetWeekMenuAsync(request.StudentId, request.AnyDateInWeek, ct);
        }
        catch (RepositoryException rex)
        {
            _logger.LogError(rex, "WeeklyMenuHandler.GetWeekMenu failed. studentId={StudentId}, date={Date}",
                request.StudentId, request.AnyDateInWeek);
            throw; // giữ nguyên để API middleware mapping ra 500 hoặc mã riêng của bạn
        }
    }

    public async Task<IReadOnlyList<WeekOptionDto>> Handle(GetAvailableWeeksQuery request, CancellationToken ct)
    {
        if (request.StudentId == Guid.Empty)
            return Array.Empty<WeekOptionDto>();

        try
        {
            return await _repo.GetAvailableWeeksAsync(request.StudentId, request.From, request.To, ct);
        }
        catch (RepositoryException rex)
        {
            _logger.LogError(rex, "WeeklyMenuHandler.GetAvailableWeeks failed. studentId={StudentId}", request.StudentId);
            throw;
        }
    }
}
