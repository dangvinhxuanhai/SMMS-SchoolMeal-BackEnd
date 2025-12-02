using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediatR;
using SMMS.Application.Features.Wardens.Commands;
using SMMS.Application.Features.Wardens.DTOs;
using SMMS.Application.Features.Wardens.Interfaces;
using SMMS.Application.Features.Wardens.Queries;

namespace SMMS.Application.Features.Wardens.Handlers;
public class WardensHealthHandler :
        IRequestHandler<GetHealthSummaryQuery, HealthSummaryDto>,
        IRequestHandler<GetStudentsHealthQuery, IEnumerable<StudentHealthDto>>,
        IRequestHandler<GetStudentBmiHistoryQuery, IEnumerable<StudentHealthDto>>,
        IRequestHandler<CreateStudentBmiCommand, StudentHealthDto>,
        IRequestHandler<UpdateStudentBmiCommand, StudentHealthDto?>,
        IRequestHandler<DeleteStudentBmiCommand, bool>
{
    private readonly IWardensHealthRepository _healthRepo;

    public WardensHealthHandler(IWardensHealthRepository healthRepo)
    {
        _healthRepo = healthRepo;
    }

    public Task<HealthSummaryDto> Handle(
        GetHealthSummaryQuery request,
        CancellationToken cancellationToken)
        => _healthRepo.GetHealthSummaryAsync(request.WardenId, cancellationToken);

    public Task<IEnumerable<StudentHealthDto>> Handle(
        GetStudentsHealthQuery request,
        CancellationToken cancellationToken)
        => _healthRepo.GetStudentsHealthAsync(request.ClassId, cancellationToken);

    public Task<IEnumerable<StudentHealthDto>> Handle(
        GetStudentBmiHistoryQuery request,
        CancellationToken cancellationToken)
        => _healthRepo.GetStudentBmiHistoryAsync(request.StudentId, cancellationToken);

    public Task<StudentHealthDto> Handle(
        CreateStudentBmiCommand request,
        CancellationToken cancellationToken)
        => _healthRepo.CreateStudentBmiAsync(
            request.StudentId,
            request.HeightCm,
            request.WeightKg,
            request.RecordDate,
            cancellationToken);

    public Task<StudentHealthDto?> Handle(
        UpdateStudentBmiCommand request,
        CancellationToken cancellationToken)
        => _healthRepo.UpdateStudentBmiAsync(
            request.RecordId,
            request.HeightCm,
            request.WeightKg,
            request.RecordDate,
            cancellationToken);

    public Task<bool> Handle(
        DeleteStudentBmiCommand request,
        CancellationToken cancellationToken)
        => _healthRepo.DeleteStudentBmiAsync(request.RecordId, cancellationToken);
}
